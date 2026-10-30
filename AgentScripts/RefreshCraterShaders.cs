using UnityEditor;using UnityEngine;
public static class RefreshCraterShaders
{
 public static object Main(){
  foreach(string file in new[]{"CityGround.shader","TankCraterSurface.shader"})AssetDatabase.ImportAsset("Assets/PhoViet/Scripts/"+file,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
  return new{success=!ShaderUtil.ShaderHasError(Shader.Find("PhoViet/City Ground"))&&!ShaderUtil.ShaderHasError(Shader.Find("PhoViet/Tank Crater Surface"))};
 }
}
