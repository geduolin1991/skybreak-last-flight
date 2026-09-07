// Offline instrument rendering. npm install spessasynth_core@4.3.22 (see package lock).
import fs from 'node:fs';import path from 'node:path';import {fileURLToPath,pathToFileURL} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const runtime=process.env.SKYBREAK_AUDIO_RUNTIME||path.resolve(root,'../.tools/skybreak-audio');
const {SpessaSynthProcessor,SoundBankLoader,SpessaLog}=await import(pathToFileURL(path.join(runtime,'node_modules/spessasynth_core/dist/index.js')));
SpessaLog.setLogLevel(false,false,false);
const raw=fs.readFileSync(path.join(runtime,'GeneralUser-GS.sf2'));const bank=SoundBankLoader.fromArrayBuffer(raw.buffer.slice(raw.byteOffset,raw.byteOffset+raw.byteLength));const sr=44100;
const manifest=JSON.parse(fs.readFileSync(path.join(root,'Tools/Score/manifest.json')));
for(const entry of manifest){
 const score=JSON.parse(fs.readFileSync(path.join(root,'Tools/Score',entry.name+'.json')));
 const synth=new SpessaSynthProcessor(sr,{eventsEnabled:false});synth.soundBankManager.addSoundBank(bank,'main');await synth.processorInitialized;
 synth.setSystemParameter('autoAllocateVoices',true);synth.setSystemParameter('reverbGain',.65);
 score.channels.forEach(([program,volume,pan,reverb],ch)=>{synth.programChange(ch,program);synth.controllerChange(ch,7,volume);synth.controllerChange(ch,10,pan);synth.controllerChange(ch,91,reverb);});
 const n=Math.round(score.seconds*sr),tail=sr*3,L=new Float32Array(n+tail),R=new Float32Array(n+tail);let e=0;
 for(let i=0;i<n+tail;i+=128){while(e<score.events.length&&score.events[e][0]*sr<=i){const [t,type,ch,p,v]=score.events[e++];if(type==='on')synth.noteOn(ch,p,v);else synth.noteOff(ch,p);}synth.process(L,R,i,Math.min(128,n+tail-i));}
 // Wrap the musical release tail into the loop head instead of an abrupt cut.
 for(let i=0;i<tail;i++){L[i]+=L[n+i];R[i]+=R[n+i];}
 const interleaved=new Float32Array(n*2);for(let i=0;i<n;i++){interleaved[i*2]=L[i];interleaved[i*2+1]=R[i];}
 const out=path.join(root,'Build/ScoreRender');fs.mkdirSync(out,{recursive:true});fs.writeFileSync(path.join(out,entry.name+'.f32'),Buffer.from(interleaved.buffer));console.log('Rendered',entry.name,score.seconds.toFixed(1)+'s',score.events.length/2+' notes');
}
