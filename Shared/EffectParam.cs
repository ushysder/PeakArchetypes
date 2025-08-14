using System.Reflection;
using UnityEngine.Rendering;

namespace PeakArchetypes.Shared;
public class EffectParam
{
	public VolumeComponent effect;
	public object originalValue;
	public PropertyInfo overrideProp;
	public PropertyInfo valueProp;
	public Volume volume;
	public object volumeParam;
}
