using System.ComponentModel;
using System.Globalization;

namespace Company.Shared.Bootstrapping
{
    [TypeConverter(typeof(EncryptedStringTypeConverter))]
    public class EncryptedString
    {
        private readonly string _rawValue;

        public EncryptedString(string value)
        {
            _rawValue = value ?? string.Empty;
            ValidateAndCheckEncryption();
        }

        private void ValidateAndCheckEncryption()
        {
            if (string.IsNullOrEmpty(_rawValue)) return;

            try
            {
                CryptoHelper.Decrypt(_rawValue);
            }
            catch
            {
                string safeEncryptedValue = CryptoHelper.Encrypt(_rawValue);

                throw new NotEncryptedException($"Is not encrpyted - set {safeEncryptedValue}");
            }
        }

        public string GetDecrypted()
        {
            if (string.IsNullOrEmpty(_rawValue)) return string.Empty;
            return CryptoHelper.Decrypt(_rawValue);
        }

        public override string ToString() => "[ENCRYPTED DATA]";
    }

    public class EncryptedStringTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return new EncryptedString(stringValue);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
