// Regenerate all install surfaces from the approved, full-square app artwork.
const fs=require('node:fs');
const path=require('node:path');
const sharp=require('sharp');
const source=path.resolve('ArtSource/Web/pho-viet-app-icon.png');
const root=path.resolve('Assets/WebGLTemplates/PhoVietPWA/icons');
async function main(){
 fs.mkdirSync(root,{recursive:true});
 for(const [name,size] of [['icon-192.png',192],['icon-512.png',512],['icon-maskable-512.png',512],['apple-touch-icon.png',180]])
  await sharp(source).resize(size,size,{fit:'cover'}).removeAlpha().png().toFile(path.join(root,name));
 const png=fs.readFileSync(path.join(root,'icon-512.png')).toString('base64');
 fs.writeFileSync(path.join(root,'icon.svg'),`<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512"><image width="512" height="512" href="data:image/png;base64,${png}"/></svg>\n`);
 console.log('Generated 192px, 512px, maskable 512px and Apple 180px icons.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
