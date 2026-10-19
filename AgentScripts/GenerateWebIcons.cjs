// Editable vector source; PNG files are platform exports, never embedded photos.
const fs=require('node:fs');
const path=require('node:path');
const sharp=require('sharp');
const source=path.resolve('ArtSource/Web/pho-viet-app-icon.svg');
const root=path.resolve('Assets/WebGLTemplates/PhoVietPWA/icons');
async function main(){
 fs.mkdirSync(root,{recursive:true});
 fs.copyFileSync(source,path.join(root,'icon.svg'));
 for(const [name,revision,size] of [
  ['icon-192.png','icon-moto-192.png',192],
  ['icon-512.png','icon-moto-512.png',512],
  ['icon-maskable-512.png','icon-moto-maskable-512.png',512],
  ['apple-touch-icon.png','apple-touch-icon-moto.png',180]]){
  const bytes=await sharp(source).resize(size,size).removeAlpha().png().toBuffer();
  fs.writeFileSync(path.join(root,name),bytes);
  fs.writeFileSync(path.join(root,revision),bytes);
 }
 console.log('Exported motorcycle vector app icon for desktop, Android and iOS.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
