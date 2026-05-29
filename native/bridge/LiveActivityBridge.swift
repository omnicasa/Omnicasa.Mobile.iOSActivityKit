//
//  LiveActivityBridge.swift
//  iOSActivityKit
//
//  ObjC-exposed control surface for ActivityKit Live Activities, callable from
//  C# (.NET for iOS / MAUI) over objc_msgSend. This file is app-agnostic: it
//  only ever deals with the generic [String: String] state defined in
//  GenericActivityAttributes, so a single compiled .xcframework works for any
//  consuming app. The app-specific UI lives in the consumer's widget extension.
//

import Foundation
import ActivityKit

@objc(LiveActivityBridge)
public class LiveActivityBridge: NSObject {

    @objc public static let shared = LiveActivityBridge()

    /// Whether Live Activities are available and the user has them enabled.
    @objc public func isEnabled() -> Bool {
        if #available(iOS 16.2, *) {
            return ActivityAuthorizationInfo().areActivitiesEnabled
        }
        return false
    }

    /// Starts a new activity. `jsonState` is a JSON object of string/string
    /// pairs. Returns the activity id, or "" on failure.
    @objc public func start(name: String, jsonState: String) -> String {
        guard #available(iOS 16.2, *) else { return "" }
        guard ActivityAuthorizationInfo().areActivitiesEnabled else {
            NSLog("[iOSActivityKit] Live Activities are not enabled")
            return ""
        }

        let attributes = GenericActivityAttributes(name: name)
        let state = GenericActivityAttributes.ContentState(data: decode(jsonState))

        do {
            let activity = try Activity.request(
                attributes: attributes,
                content: .init(state: state, staleDate: nil),
                pushType: nil
            )
            NSLog("[iOSActivityKit] started '\(name)' -> \(activity.id)")
            return activity.id
        } catch {
            NSLog("[iOSActivityKit] start '\(name)' failed: \(error.localizedDescription)")
            return ""
        }
    }

    /// Updates every running activity whose name matches.
    @objc public func update(name: String, jsonState: String) {
        guard #available(iOS 16.2, *) else { return }
        let state = GenericActivityAttributes.ContentState(data: decode(jsonState))

        Task {
            for activity in Activity<GenericActivityAttributes>.activities
            where activity.attributes.name == name {
                await activity.update(ActivityContent(state: state, staleDate: nil))
            }
        }
    }

    /// Ends every running activity whose name matches. If `jsonState` is empty
    /// the activity's last content is kept. `dismissAfter` (seconds) controls
    /// how long the finished activity lingers; 0 uses the system default.
    @objc public func end(name: String, jsonState: String, dismissAfter: Double) {
        guard #available(iOS 16.2, *) else { return }
        let hasFinalState = !jsonState.isEmpty
        let finalState = GenericActivityAttributes.ContentState(data: decode(jsonState))

        Task {
            for activity in Activity<GenericActivityAttributes>.activities
            where activity.attributes.name == name {
                let policy: ActivityUIDismissalPolicy = dismissAfter > 0
                    ? .after(Date().addingTimeInterval(dismissAfter))
                    : .default
                if hasFinalState {
                    await activity.end(
                        ActivityContent(state: finalState, staleDate: nil),
                        dismissalPolicy: policy
                    )
                } else {
                    await activity.end(nil, dismissalPolicy: policy)
                }
            }
        }
    }

    /// Immediately ends all activities of this type, regardless of name.
    @objc public func endAll() {
        guard #available(iOS 16.2, *) else { return }
        Task {
            for activity in Activity<GenericActivityAttributes>.activities {
                await activity.end(nil, dismissalPolicy: .immediate)
            }
        }
    }

    private func decode(_ json: String) -> [String: String] {
        guard !json.isEmpty,
              let data = json.data(using: .utf8),
              let dict = try? JSONDecoder().decode([String: String].self, from: data)
        else {
            return [:]
        }
        return dict
    }
}
