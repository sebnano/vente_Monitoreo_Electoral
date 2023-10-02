using System;
namespace ElectoralMonitoring
{
	public interface IFieldControl
	{
		object GetValue();
        void SetValue(object value);
		bool HasValue();
		string GetKey();
        string Key { get; set; }
    }
}

