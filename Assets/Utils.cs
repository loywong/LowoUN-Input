namespace LowoUN.Util {
    public abstract class SingletonSimple<T> where T : new () {
        private static T self;
        public static T Self {
            get {
                if (self == null)
                    self = new T ();
                return self;
            }
        }
    }
}