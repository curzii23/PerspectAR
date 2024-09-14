
let ws;

function openWebSocket(type, userName, userType, stype, bvirtual, myGaze, PID="none", UID="none") {
  if (!ws) {
    ws = new WebSocket('ws://localhost:3000');
  }

  ws.addEventListener('open', (event) => {

    //Initilize Startup Parameters
    if(bvirtual == true)
    {
      sendMessageDynamic({type:'smessage', sCode:"config", gazeStatus:myGaze, userType:userType, sType:stype, userName:userName, PID:PID, UID:UID});//Smartphone to Server to Unity (configuration)
      sendMessageDynamic({type:type, userType:userType, stype:stype, userName:userName, PID:PID, UID:UID});
    }
    else{
      sendMessageDynamic({type:type, userType:userType, sType:stype, userName:userName, PID:PID, UID:UID});
    }
    

  });

  ws.addEventListener('error', (error) => {
    console.error('WebSocket error:', error);
  });
  
  ws.addEventListener('close', (event) => {
    console.log('WebSocket closed:', event);
    ws = null;
  });

}

//Calibration
function sendMessageCalib(type, temp1, temp2, temp3) {//type (system, ssmessage), message, userType (LD or AR), sType (Pilot or Study)

  ws = new WebSocket('ws://localhost:3000');

  ws.addEventListener('open', (event) => {

    sendMessageDynamic({
      type: 'calibInfoType',
      sCode: type,
      DepthZ: temp1,
      Angle: temp2,
      Scale: temp3
  });
});
}



function sendMessageDynamic(data) {
  
  
  if (typeof data !== 'object' || !data.type) {
      console.error('Invalid data provided to sendMessage');
      return;
  }

  const jsonData = JSON.stringify(data);
  ws.send(jsonData);
}