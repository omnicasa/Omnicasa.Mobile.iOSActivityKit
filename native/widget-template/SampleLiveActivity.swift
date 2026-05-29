//
//  SampleLiveActivity.swift
//  iOSActivityKit — widget extension template
//
//  This is the ONLY file you really customize. It defines how your Live
//  Activity looks on the Lock Screen and in the Dynamic Island. The data comes
//  from the generic [String: String] dictionary you pass from C#:
//
//      context.state.data["progress"]   // "0.42"
//      context.state.data["phase"]      // "Downloading"
//      context.attributes.name          // "sync"  (set in Start("sync", ...))
//
//  Rename freely, but keep `ActivityConfiguration(for: GenericActivityAttributes.self)`.
//

import ActivityKit
import WidgetKit
import SwiftUI

struct SampleLiveActivity: Widget {
    var body: some WidgetConfiguration {
        ActivityConfiguration(for: GenericActivityAttributes.self) { context in
            // ---- Lock Screen / banner ----
            LockScreenView(context: context)
                .padding()
                .activityBackgroundTint(Color.black.opacity(0.6))
        } dynamicIsland: { context in
            DynamicIsland {
                DynamicIslandExpandedRegion(.leading) {
                    Text(context.attributes.name.capitalized)
                        .font(.caption.bold())
                }
                DynamicIslandExpandedRegion(.trailing) {
                    Text(percentText(context))
                        .font(.headline)
                        .monospacedDigit()
                }
                DynamicIslandExpandedRegion(.bottom) {
                    VStack(alignment: .leading, spacing: 6) {
                        ProgressView(value: progress(context))
                        if let message = context.state.data["message"], !message.isEmpty {
                            Text(message)
                                .font(.caption2)
                                .foregroundStyle(.secondary)
                        }
                    }
                }
            } compactLeading: {
                Text(context.attributes.name.prefix(1).uppercased())
            } compactTrailing: {
                Text(percentText(context))
                    .monospacedDigit()
            } minimal: {
                Text(percentText(context))
                    .monospacedDigit()
            }
        }
    }

    private func progress(_ context: ActivityViewContext<GenericActivityAttributes>) -> Double {
        Double(context.state.data["progress"] ?? "0") ?? 0
    }

    private func percentText(_ context: ActivityViewContext<GenericActivityAttributes>) -> String {
        "\(Int(progress(context) * 100))%"
    }
}

private struct LockScreenView: View {
    let context: ActivityViewContext<GenericActivityAttributes>

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            HStack {
                Text(context.state.data["phase"] ?? context.attributes.name)
                    .font(.headline)
                Spacer()
                Text("\(Int((Double(context.state.data["progress"] ?? "0") ?? 0) * 100))%")
                    .font(.headline)
                    .monospacedDigit()
            }
            ProgressView(value: Double(context.state.data["progress"] ?? "0") ?? 0)
            if let message = context.state.data["message"], !message.isEmpty {
                Text(message)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
    }
}
