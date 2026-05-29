//
//  SampleWidgetBundle.swift
//  iOSActivityKit — widget extension template
//
//  The entry point of the widget extension. List every widget the extension
//  provides here. For Live Activities you only need the activity widget, but
//  you can add Home Screen / Lock Screen widgets to the same bundle.
//

import WidgetKit
import SwiftUI

@main
struct SampleWidgetBundle: WidgetBundle {
    var body: some Widget {
        SampleLiveActivity()
    }
}
