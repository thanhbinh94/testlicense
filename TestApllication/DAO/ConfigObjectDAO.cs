namespace TestApllication.DAO
{
    public class ConfigObjectDAO
    {
        private int configNumberDigitPerUnit = 5;   // For generate license
        private int configNumberOfUnit = 5;
        private string configLicenseSuffix = "-";
        private int maxInputTimes = 4;

        public int ConfigNumberDigitPerUnit
        {
            get
            {
                return configNumberDigitPerUnit;
            }
            set
            {
                configNumberDigitPerUnit = value;
            }
        }

        public int ConfigNumberOfUnit
        {
            get
            {
                return configNumberOfUnit;
            }
            set
            {
                configNumberOfUnit = value;
            }
        }

        public string ConfigLicenseSuffix
        {
            get
            {
                return configLicenseSuffix;
            }
            set
            {
                configLicenseSuffix = value;
            }
        }

        public int ConfigMaxInputTimes
        {
            get
            {
                return maxInputTimes;
            }
            set
            {
                maxInputTimes = value;
            }
        }
    }
}
