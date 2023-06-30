// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Microsoft.Toolkit.Uwp.Connectivity
{
    /// <summary>
    /// Helper function when working with <see cref="GattProtocolError" />
    /// </summary>
    public static class GattProtocolErrorParser
    {
        /// <summary>
        /// Helper to convert an Gatt error value into a string
        /// </summary>
        /// <param name="errorValue"> the byte error value.</param>
        /// <returns>String representation of the error</returns>
        public static string GetErrorString(this byte? errorValue)
        {
            string errorString = "Protocol Error";

            return errorValue.HasValue == false
                ? errorString
                : errorValue == GattProtocolError.AttributeNotFound
                ? "Attribute Not Found"
                : errorValue == GattProtocolError.AttributeNotLong
                ? "Attribute Not Long"
                : errorValue == GattProtocolError.InsufficientAuthentication
                ? "Insufficient Authentication"
                : errorValue == GattProtocolError.InsufficientAuthorization
                ? "Insufficient Authorization"
                : errorValue == GattProtocolError.InsufficientEncryption
                ? "Insufficient Encryption"
                : errorValue == GattProtocolError.InsufficientEncryptionKeySize
                ? "Insufficient Encryption Key Size"
                : errorValue == GattProtocolError.InsufficientResources
                ? "Insufficient Resources"
                : errorValue == GattProtocolError.InvalidAttributeValueLength
                ? "Invalid Attribute Value Length"
                : errorValue == GattProtocolError.InvalidHandle
                ? "Invalid Handle"
                : errorValue == GattProtocolError.InvalidOffset
                ? "Invalid Offset"
                : errorValue == GattProtocolError.InvalidPdu
                ? "Invalid Pdu"
                : errorValue == GattProtocolError.PrepareQueueFull
                ? "Prepare Queue Full"
                : errorValue == GattProtocolError.ReadNotPermitted
                ? "Read Not Permitted"
                : errorValue == GattProtocolError.RequestNotSupported
                ? "Request Not Supported"
                : errorValue == GattProtocolError.UnlikelyError
                ? "UnlikelyError"
                : errorValue == GattProtocolError.UnsupportedGroupType
                ? "Unsupported Group Type"
                : errorValue == GattProtocolError.WriteNotPermitted
                ? "Write Not Permitted"
                : errorString;
        }
    }
}