namespace Microtopia.Tests
{
    public static class Converter
    {
        public static T ConvertTo<T>(this object a) where T : new()
        {
            return (T) a;
        }
    }
}