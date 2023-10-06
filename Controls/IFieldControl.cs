using System;
namespace ElectoralMonitoring
{
	public interface IFieldControl
	{
		object GetValue();
        void SetValue(object value);
		bool HasValue();
        bool IsRequired();
        string GetKey();
        void SetRequiredStatus();
        string Key { get; set; }
    }
}

