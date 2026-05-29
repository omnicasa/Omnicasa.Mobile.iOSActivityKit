#!/bin/bash
#
# Builds LiveActivityBridge.xcframework for use as a <NativeReference> in a
# .NET for iOS / MAUI project. The framework bundles both the bridge and the
# shared GenericActivityAttributes type, so consuming apps never need to edit
# or recompile any Swift to control Live Activities.
#
# Usage: ./build.sh
# Output: ./output/LiveActivityBridge.xcframework
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SHARED_DIR="$(cd "${SCRIPT_DIR}/../shared" && pwd)"
FRAMEWORK_NAME="iOSActivityKitBridge"
BUILD_DIR="${SCRIPT_DIR}/build"
OUTPUT_DIR="${SCRIPT_DIR}/output"
MIN_IOS="16.2"

SOURCES=(
    "${SHARED_DIR}/GenericActivityAttributes.swift"
    "${SCRIPT_DIR}/LiveActivityBridge.swift"
)

IPHONEOS_SDK="$(xcrun --sdk iphoneos --show-sdk-path)"
IPHONESIMULATOR_SDK="$(xcrun --sdk iphonesimulator --show-sdk-path)"

rm -rf "${BUILD_DIR}" "${OUTPUT_DIR}"
mkdir -p "${BUILD_DIR}/device" "${BUILD_DIR}/simulator" "${OUTPUT_DIR}"

build_slice() {
    local dest="$1" target="$2" sdk="$3"
    echo "Building ${FRAMEWORK_NAME} for ${dest} (${target})..."
    xcrun swiftc \
        -emit-library \
        -emit-module \
        -emit-module-interface \
        -enable-library-evolution \
        -module-name "${FRAMEWORK_NAME}" \
        -target "${target}" \
        -sdk "${sdk}" \
        -O \
        -Xlinker -install_name -Xlinker "@rpath/${FRAMEWORK_NAME}.framework/${FRAMEWORK_NAME}" \
        -o "${BUILD_DIR}/${dest}/lib${FRAMEWORK_NAME}.dylib" \
        "${SOURCES[@]}"
}

build_slice "device"    "arm64-apple-ios${MIN_IOS}"            "${IPHONEOS_SDK}"
build_slice "simulator" "arm64-apple-ios${MIN_IOS}-simulator" "${IPHONESIMULATOR_SDK}"

echo "Creating framework structures..."
DEVICE_FW="${BUILD_DIR}/device/${FRAMEWORK_NAME}.framework"
SIM_FW="${BUILD_DIR}/simulator/${FRAMEWORK_NAME}.framework"

assemble_framework() {
    local fw="$1" src="$2" arch="$3"
    mkdir -p "${fw}/Modules/${FRAMEWORK_NAME}.swiftmodule"
    cp "${BUILD_DIR}/${src}/lib${FRAMEWORK_NAME}.dylib" "${fw}/${FRAMEWORK_NAME}"
    install_name_tool -id "@rpath/${FRAMEWORK_NAME}.framework/${FRAMEWORK_NAME}" "${fw}/${FRAMEWORK_NAME}"
    cp "${BUILD_DIR}/${src}/${FRAMEWORK_NAME}.swiftmodule" \
       "${fw}/Modules/${FRAMEWORK_NAME}.swiftmodule/${arch}.swiftmodule" 2>/dev/null || true
    cp "${BUILD_DIR}/${src}/${FRAMEWORK_NAME}.swiftinterface" \
       "${fw}/Modules/${FRAMEWORK_NAME}.swiftmodule/${arch}.swiftinterface" 2>/dev/null || true
    cat > "${fw}/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>${FRAMEWORK_NAME}</string>
    <key>CFBundleIdentifier</key>
    <string>io.github.iosactivitykit.bridge</string>
    <key>CFBundleName</key>
    <string>${FRAMEWORK_NAME}</string>
    <key>CFBundlePackageType</key>
    <string>FMWK</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>MinimumOSVersion</key>
    <string>${MIN_IOS}</string>
</dict>
</plist>
PLIST
}

assemble_framework "${DEVICE_FW}" "device"    "arm64-apple-ios"
assemble_framework "${SIM_FW}"    "simulator" "arm64-apple-ios-simulator"

echo "Creating xcframework..."
xcodebuild -create-xcframework \
    -framework "${DEVICE_FW}" \
    -framework "${SIM_FW}" \
    -output "${OUTPUT_DIR}/${FRAMEWORK_NAME}.xcframework"

echo ""
echo "Done! -> ${OUTPUT_DIR}/${FRAMEWORK_NAME}.xcframework"
