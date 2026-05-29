//
//  GenericActivityAttributes.swift
//  iOSActivityKit
//
//  The single, app-agnostic ActivityAttributes type shared by BOTH
//  the prebuilt bridge framework and the consumer's widget extension.
//
//  ⚠️  This exact file must be compiled into your widget extension as well.
//  ActivityKit matches a running activity to its widget UI by the *name* of
//  the ActivityAttributes type ("GenericActivityAttributes") and the shape of
//  its Codable ContentState. Do not rename the type or change the fields, or
//  the bridge and the widget will no longer match.
//
//  The state is intentionally a flat [String: String] dictionary so a single
//  prebuilt bridge can carry arbitrary, app-defined values without recompiling.
//  Read the values in your SwiftUI with `context.state.data["yourKey"]` and
//  branch your layout on `context.attributes.name` if you host more than one
//  kind of activity.
//

import ActivityKit

public struct GenericActivityAttributes: ActivityAttributes {
    public struct ContentState: Codable, Hashable {
        /// Arbitrary, app-defined values for the current frame of the activity.
        public var data: [String: String]

        public init(data: [String: String]) {
            self.data = data
        }
    }

    /// Logical name of the activity ("sync", "delivery", …). Lets one app run
    /// several distinct activities off the same attributes type and lets the
    /// widget pick a layout. Also used by the bridge to target update/end.
    public var name: String

    public init(name: String) {
        self.name = name
    }
}
