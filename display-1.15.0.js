/* The camera and animated radio share a canvas, but never share screen space.
   Reserve the dock even during silence so dialogue cannot resize the battle. */
window.createSkybreakDisplay = function ({canvas,stage,mobile,onLayout}) {
 const profiles={balanced:{scale:1.5,pixels:850000},clear:{scale:2,pixels:1150000},smooth:{scale:1,pixels:500000}};
 let quality='balanced';
 try{const saved=localStorage.getItem('skybreak-quality');if(Object.hasOwn(profiles,saved))quality=saved;}catch{}
 let timer=0,resizes=0,lastLayout='',battle=false;
 function apply(){
  if(!mobile)return;
  const box=stage.getBoundingClientRect();if(box.width<1||box.height<1)return;
  const portrait=box.height>box.width;
  stage.classList.toggle('portrait-layout',portrait);stage.classList.toggle('battle-layout',battle);
  const hud=battle?(portrait?104:64):0;
  const deck=battle&&portrait?(box.height<710?164:174):0;
  const radio=battle&&portrait?104:0;
  const available=Math.max(80,box.height-hud-deck-radio);
  // Portrait uses the whole available width. A fixed 3:4 box used to become a
  // narrow postage stamp on short phones and hid UI without restoring scenery.
  const height=battle&&portrait?available:Math.min(available,box.width/(portrait?box.width/box.height:16/9));
  const width=battle&&portrait?box.width:portrait?box.width:height*16/9;
  const canvasHeight=height+radio,x=(box.width-width)/2,y=battle?hud:(box.height-height)/2;
  const layout=[width,canvasHeight,x,y,box.width,box.height,radio].map(n=>n.toFixed(3)).join(':');
  if(layout!==lastLayout){
   lastLayout=layout;canvas.style.width=width+'px';canvas.style.height=canvasHeight+'px';canvas.style.left=x+'px';canvas.style.top=y+'px';
   for(const [key,value] of Object.entries({'game-width':width,'game-height':height,'game-inset-x':x,'game-inset-y':y,'game-bottom':y+height,'hud-height':hud,'radio-height':radio,'deck-height':deck}))stage.style.setProperty('--'+key,value+'px');
  }
  const profile=profiles[quality],scale=Math.min(window.devicePixelRatio||1,profile.scale,Math.sqrt(profile.pixels/(width*canvasHeight)));
  let rw,rh;
  if(!portrait){const units=Math.max(1,Math.floor(width*scale/16));rw=units*16;rh=units*9;}
  else{rw=Math.max(2,Math.floor(width*scale/2)*2);rh=Math.max(2,Math.round(rw*canvasHeight/width/2)*2);}
  if(canvas.width!==rw||canvas.height!==rh){canvas.width=rw;canvas.height=rh;resizes++;}
  // Share normalized bounds with Unity. Integer framebuffer rounding stays
  // below one CSS pixel at odd viewport and safe-area dimensions.
  const viewport={x:0,y:radio/canvasHeight,w:1,h:height/canvasHeight};
  window.skybreakDisplayState={quality,resizes,width:rw,height:rh,cssWidth:width,cssHeight:canvasHeight,fieldWidth:width,fieldHeight:height,portrait,battle,hud,deck,radio,viewport};
  requestAnimationFrame(()=>onLayout?.());
 }
 function schedule(){clearTimeout(timer);timer=setTimeout(apply,160);}
 if(mobile){apply();new ResizeObserver(schedule).observe(stage);window.addEventListener('orientationchange',schedule);window.addEventListener('resize',schedule);document.addEventListener('fullscreenchange',schedule);}
 return {
  get quality(){return quality;},
  unityConfig:mobile?{matchWebGLToCanvasSize:false,devicePixelRatio:1}:{devicePixelRatio:Math.min(window.devicePixelRatio||1,1.25)},
  setQuality(next){if(!Object.hasOwn(profiles,next)||next===quality)return;quality=next;try{localStorage.setItem('skybreak-quality',quality);}catch{}apply();},
  setBattle(next){if(battle===next)return;battle=next;apply();},
  refresh:schedule
 };
};
