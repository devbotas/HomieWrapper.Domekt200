using System;
namespace SharpModbus {
    public static class Tools {
        public static void AssertEqual(int a, int b, string format) {
            if (a != b) Tools.Throw(format, a, b);
        }

        public static void Throw(string format, params object[] args) {
            var message = format;
            if (args.Length > 0) message = string.Format(format, args);
            throw new Exception(message);
        }
    }
}
