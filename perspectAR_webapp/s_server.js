const http = require('http');
const WebSocket = require('ws');
const sqlite3 = require('sqlite3').verbose();
const XLSX = require('xlsx');
const _ = require('lodash');

let readyParticipants = [];
let numberofParticipants = 2;
var iSCurrentCondition = 0;//Single User
var iDCurrentCondition = 0;//Pair User

// Create a new SQLite database and table
const db = new sqlite3.Database('participants.db');
db.run('CREATE TABLE IF NOT EXISTS responses (userName TEXT, userType TEXT, PID TEXT, UID TEXT, cTime TEXT, sType TEXT, completionTime INTEGER, qindex TEXT, answer TEXT, Flip TEXT, Toggle TEXT)');

// Create a WebSocket server
const server = http.createServer();
const wss = new WebSocket.Server({ server });

// Store connected users in a Map
const userConnections = new Map();
const systemUsers = new Map();
const virtualUsers = new Map();

let currentSeed = Math.random(); // Initial seed
const bCalib = false;

// Load the workbook
const workbook = XLSX.readFile('data.xlsx');

// Convert the first worksheet into JSON
const sheetName = workbook.SheetNames[0];
const worksheet = workbook.Sheets[sheetName];
const data = XLSX.utils.sheet_to_json(worksheet);

// Initialize the tasks object
let tasks = {};

// Populate the tasks object
data.forEach(row => {
    // Destructure row into its components
    const { Phase, Category, Index, Value, Hint, Image } = row;

    // Initialize phase if it doesn't exist
    if (!tasks[Phase]) {
        tasks[Phase] = {};
    }

    // Initialize category within phase if it doesn't exist
    if (!tasks[Phase][Category]) {
        tasks[Phase][Category] = [];
    }

    // Construct the task object
    const task = { index: Index, value: Value };
    if (Hint) task.hint = Hint;
    if (Image) task.image = Image;

    // Add the task object to the appropriate category within phase
    tasks[Phase][Category].push(task);
});


const sequence = ['WEATHER', 'none', 'Search'];
//const sequence = ['Search', 'none', 'WEATHER'];

let currentTask = [];
var taskStart = 0;
var subTasks = 0;
var currentActiveTask = '';
let qIndex = "";
let path = "";
let gimage = '';
let ghint = "";

function resetVariables() {
    subTasks = 0;
    taskStart = 0;
    currentTask = [];
    randomQuestion = '';
    currentActiveTask = 'none';
    qIndex = '';
    path = '';
    image = '';
    ghint = '';
    iSCurrentCondition = 0;//Single User
    iDCurrentCondition = 0;//Pair User
}

// Function to generate tasks based on study type
function GenTask(studyType) {

    if (taskStart >= sequence.length) {
        return null;
    }
    
    currentActiveTask = sequence[taskStart];
    currentTask = tasks[studyType][currentActiveTask];
    
    if (subTasks >= currentTask.length) {

        taskStart++;
        subTasks = 0;

        if (taskStart === sequence.length) {
            return null;
        }
    }

    currentActiveTask = sequence[taskStart];
    currentTask = tasks[studyType][currentActiveTask][subTasks++];
    const { index, value, image = "", hint = "" } = currentTask;

    qIndex = index;
    path = value;
    gimage = image;
    ghint = hint;

    return value;
}


// Start the server
const port = 3000;
server.listen(port, () => {
    console.log(`Server listening on port ${port}`);
});

//Check Condition
function CheckCondition() {

    const userIds = Array.from(userConnections.keys());

    //single user is just for testing
    if (userConnections.size == 1)//Single User
    {
        if (userIds[0].includes("ULD"))//Large Display
        {
            iSCurrentCondition = 1;
            numberofParticipants = 1;
        }
        if (userIds[0].includes("UAR"))//AR
        {
            iSCurrentCondition = 2;
            numberofParticipants = 1;
        }
    }

    if (userConnections.size == 2)//Two Users
    {
        if (userIds[0].includes("ULD") && userIds[1].includes("ULD"))//Large Display + Large Display
        {
            iDCurrentCondition = 1;
            numberofParticipants = 2;
        }
        if (userIds[0].includes("UAR") && userIds[1].includes("UAR")) {
            iDCurrentCondition = 2;
            numberofParticipants = 2;
        }
        //We might not use this
        if (userIds[0].includes("UAR") && userIds[1].includes("ULD") || userIds[0].includes("ULD") && userIds[1].includes("UAR")) {
            iDCurrentCondition = 3;
            numberofParticipants = 2;
        }
    }
}


//2 users through smartphones
function addUsers(userID, ws) {

    console.log("User Connected: " + userID);
    
    if (userConnections.size > 0) {
        if (userConnections.has(userID)) {
            userConnections.delete(userID);
        }
    }

    // Add the WebSocket connection to the user's connection list
    if (!userConnections.has(userID)) {
        userConnections.set(userID, ws);
    }

}

//3 systems users (1 large display, 2 webview hololens) websites
function addSystemUsers(userID, ws) {

    console.log("System User: " + userID);

    if (systemUsers.size > 0) {
        if (systemUsers.has(userID)) {
            systemUsers.delete(userID);
        }
    }

    // Add the WebSocket connection to the user's connection list
    if (!systemUsers.has(userID)) {
        systemUsers.set(userID, ws);
    }

    if (bCalib) {
        ws.send(currentSeed.toString());
    }


}

//2 hololens users
function addVirtualUsers(userID, ws) {

    
    console.log("Virtual User: " + userID);

    if (virtualUsers.size > 0) {
        if (virtualUsers.has(userID)) {
            virtualUsers.delete(userID);
        }
    }

    // Add the WebSocket connection to the user's connection list
    if (!virtualUsers.has(userID)) {

        virtualUsers.set(userID, ws);
    }
}


wss.on('connection', (ws) => {

    // Handle incoming messages from clients
    ws.on('message', (message, isBinary) => {

        try {

            const data = JSON.parse(message);

            if (data.type == 'ready') {//start of the study


                const { userType, stype } = data;

                addUsers(userType, ws);

                // Check if all users are ready and send the update message to them
                if (userConnections.size == numberofParticipants) {

                    CheckCondition();
                    sendUsers(stype);
                    SendtoVirtualUsers(currentActiveTask, userType);

                }
                else {
                    ws.send(JSON.stringify({ type: 'wait' }));
                }

            }

            if (data.type === 'answer') {//update after receiving answer

                const { userName, userType, stype, completionTime, answer, PID, UID, Flip, ToggleTimes } = data;
                // Get the current date and time
                const currentTime = new Date().toLocaleString();//.toISOString()

                // Save the response to the database
                db.run('INSERT INTO responses (userName, userType, PID, UID, cTime, sType, completionTime, qindex, answer, Flip, Toggle) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)',
                    [userName, userType, PID, UID, currentTime, stype, completionTime, qIndex, answer, Flip, ToggleTimes], (error) => {
                        if (error) {
                            console.error('Error inserting data:', error.message);
                        } else {
                            console.log('Data inserted successfully');
                        }
                    });

                readyParticipants.push(ws);

                if (readyParticipants.length !== numberofParticipants) {

                    // update cannot be performed yet
                    ws.send(JSON.stringify({ type: 'wait' }));

                }
                else {

                    readyParticipants = [];

                    // Check if all users are ready and send the update message to them
                    if (userConnections.size == numberofParticipants) {

                        sendUsers(stype);
                        SendtoVirtualUsers(currentActiveTask, userType);

                    }
                }
            }

            if (data.type === 'begin') {//after break

                const { userType, stype } = data;
                readyParticipants.push(ws);

                if (readyParticipants.length !== numberofParticipants) {

                    // Display update cannot be performed yet
                    ws.send(JSON.stringify({ type: 'wait' }));

                }
                else {

                    readyParticipants = [];

                    // Check if all users are ready and send the update message to them
                    if (userConnections.size == numberofParticipants) {

                  
                        sendUsers(stype);//Update Users on Smartphone
                        SendtoVirtualUsers(currentActiveTask, userType);//Current task inform to virtual users

                    }

                }


            }

            //Add System (Large Display) Users and Virtual (HoloLens) Users
            if (data.type == 'system') {

                const { userName } = data;

                const messageString = userName;

                if (messageString.startsWith('ID:')) {//ADD Large Display User and Add HoloLens Webbrowser Users (Virtual) (3)

                    const userID = messageString.substring(3);
                    addSystemUsers(userID, ws);

                }

                if (messageString.startsWith('VID:')) {//Add Unity Users (HoloLens)

                    const userID = messageString.substring(4);
                    addVirtualUsers(userID, ws);
                }
            }

            //From users to system users and virtual users (messages, such as configuration)
            if (data.type == 'smessage') {

                //both none and not-none messages to virtual users (Unity First and Second), such first time config, disable/enable mygaze, and PerspectAR
                SendToHLDeviceLM(data);

            }
            if (data.type == 'sfeature') {

                if(iSCurrentCondition == 2 || iDCurrentCondition == 2)
                {
                    swapWeatherRows(data);
                }
                else
                {
                    swapWeatherRowsLargeDisplay(data);
                }
            }

            //From users to system users and virtual users (messages, such as configuration)
            if (data.type == 'calibInfoType') {

                //both none and not-none to virtual users (Unity First and Second)
                SendtoVirtualUsersCalib(data);
            }

            if (iSCurrentCondition == 2 || iDCurrentCondition == 2) {
                //From users to system users and virtual users (messages, such as configuration)
                if (data.type == 'bbox' || data.type == 'temp') {
                    //both none and not-none to virtual users (Unity First and Second)
                    sendInteractionStatus(data);
                }
            }

            if(iDCurrentCondition == 2 || iDCurrentCondition == 1)
            {
                if (data.type === 'gazedataunityFirst') {
    
                    SendToHLDeviceReverse(data);
                }
                if (data.type === 'gazedataunitySecond') {
      
                    SendToHLDeviceReverse(data);
                }
            }
            
        }

        catch (error) {
            console.error('Error parsing JSON:', error);

        }
    });

    ws.on('close', function () {

        resetVariables();
        // Remove user information when they disconnect
        userConnections.forEach((user, userID) => {


            if (user === ws) {
                console.log(`User ${userID} disconnected`);
                userConnections.delete(userID);
            }

        });

        // Remove user information when they disconnect
        systemUsers.forEach((user, userID) => {

       
            if (user === ws) {
                console.log(`System User ${userID} disconnected`);
                systemUsers.delete(userID);
            }

        });

        // Remove user information when they disconnect
        virtualUsers.forEach((user, userID) => {

            if (user === ws) {
                console.log(`Virtual User ${userID} disconnected`);
                virtualUsers.delete(userID);
            }

        });


        ws.on('error', function (error) {
            console.error('WebSocket error:', error);
        });

    });


});

//from users (smartphone) to virtual users (HL)//First time configuration and setup
function SendToHLDeviceLM(message) {


    if (message.userType.includes("First")) {
        
        const userIds = Array.from(virtualUsers.keys());

        userIds.forEach((id) => {

            const userWs = virtualUsers.get(id);//virtual users

            if (id.includes("First")) {
                userWs.send(JSON.stringify(message));
            }

        });
    }

    if (message.userType.includes("Second")) {

        const userIds = Array.from(virtualUsers.keys());

        userIds.forEach((id) => {

            const userWs = virtualUsers.get(id);

            if (id.includes("Second")) {
                userWs.send(JSON.stringify(message));
            }

        });
    }
}

//Sending data such as geospatial bounding box, and weather chart temp data to HoloLens 2 websites
function sendInteractionStatus(message) {

    const userIds = Array.from(systemUsers.keys());
    // Send the same random question to each user
    userIds.forEach((id) => {

        if (id != "HL") {
            const userWs = systemUsers.get(id);
            userWs.send(JSON.stringify(message));
        }
    });
}

//Swap weather chart rows on all the websites (Large Display and HoloLens2)
function swapWeatherRows(message) {

    const userIds = Array.from(systemUsers.keys());
    // Send the same random question to each user
    userIds.forEach((id) => {
        const userWs = systemUsers.get(id);
        userWs.send(JSON.stringify({ type: message.type, message: message.message }));
    });

    
}

function swapWeatherRowsLargeDisplay(message) {

    const userIds = Array.from(systemUsers.keys());

    // Send the same random question to each user
    userIds.forEach((id) => {

        if (id == "HL") {

            const userWs = systemUsers.get(id);
            userWs.send(JSON.stringify({ type: message.type, message: message.message }));
        }   
    });
}


//Send Users Update the information on users' smartphones and Update the contents on Large Display and HoloLens Websites
//Physical and Virtual Large Display Users (Physical and Virtual) to modify/update content
function sendUsers(stype) {
    const randomQuestion = GenTask(stype);

    if (randomQuestion != null) {

        updateLDandARR('system');//update content on systems (HoloLens == the websites)

        const userIds = Array.from(userConnections.keys());
        // Send the same random question to each user (mobile phones)
        userIds.forEach((id) => {
            const userWs = userConnections.get(id);
            userWs.send(JSON.stringify({ type: 'update', qindex: qIndex, value: randomQuestion, image: gimage, hint: ghint }));
        });
    }
    else {
        updateLDandARR('done');//update content on systems (system)

        const userIds = Array.from(userConnections.keys());
        // Send the same random question to each user
        userIds.forEach((id) => {
            const userWs = userConnections.get(id);
            userWs.send(JSON.stringify({ type: 'done', qindex: qIndex, value: randomQuestion, image: gimage, hint: ghint }));
        });
        [sequence[0], sequence[2]] = [sequence[2], sequence[0]];
        resetVariables();

    }
}

function updateLDandARR(message) {

    console.log("Current Condition Single User:" + iSCurrentCondition);
    console.log("Current Condition Double User:" + iDCurrentCondition);

    handleSingleUserCases(message);
    handlePairUsersCases(message);
}

function handleSingleUserCases(message) {
    if (iSCurrentCondition == 1) {
        const userIds = Array.from(systemUsers.keys());
        // Send the same random question to each user
        userIds.forEach((id) => {
            const userWs = systemUsers.get(id);

            if (id.includes("HL")) {
                userWs.send(JSON.stringify({ type: message, qindex: qIndex, value: path, image: gimage, hint: ghint }));
            }

        });
    }
    if (iSCurrentCondition == 2) {
        const userIds = Array.from(systemUsers.keys());
        // Send the same random question to each user
        userIds.forEach((id) => {
            const userWs = systemUsers.get(id);
            userWs.send(JSON.stringify({ type: message, qindex: qIndex, value: path, image: gimage, hint: ghint }));
        });
    }
}

function handlePairUsersCases(message) {
    if (iDCurrentCondition == 1) {// both on the display
        const userIds = Array.from(systemUsers.keys());
        // Send the same random question to each user
        userIds.forEach((id) => {
            const userWs = systemUsers.get(id);

            if (id.includes("HL")) {
                userWs.send(JSON.stringify({ type: message, qindex: qIndex, value: path, image: gimage, hint: ghint }));
            }
        });
    }
    if (iDCurrentCondition == 2) {//both using the PerspectAR with the large display
        const userIds = Array.from(systemUsers.keys());
        // Send the same random question to each user
        userIds.forEach((id) => {
            const userWs = systemUsers.get(id);
            userWs.send(JSON.stringify({ type: message, qindex: qIndex, value: path, image: gimage, hint: ghint }));
        });
    }

    
}
//End


//Calibration
function SendtoVirtualUsersCalib(message) {

    const userIds = Array.from(virtualUsers.keys());

    userIds.forEach((id) => {

        const userWs = virtualUsers.get(id);//virtual users

        if (id.includes("First")) {
            userWs.send(JSON.stringify(message));
        }

    });

}


//Hololens websockets
//server to virtual users (unity) to inform the task type (we only save data during each task)
function SendtoVirtualUsers(message, userType) {

    const userIds = Array.from(virtualUsers.keys());

    userIds.forEach((id) => {

        const userWs = virtualUsers.get(id);//virtual users

        if (id.includes("First")) {
            userWs.send(JSON.stringify({ type: "smessage", sCode: "cTask", taskType: message.toString(), taskIndex: qIndex.toString() }));
        }

    });

    userIds.forEach((id) => {

        const userWs = virtualUsers.get(id);

        if (id.includes("Second")) {
            userWs.send(JSON.stringify({ type: "smessage", sCode: "cTask", taskType: message.toString(), taskIndex: qIndex.toString() }));
        }

    });


    
}

//from virtual users (HL) to virtual users (HL) (Gaze Data)
function SendToHLDeviceReverse(message) {

    if (message.type.includes("First")) {

        const userIds = Array.from(virtualUsers.keys());

        userIds.forEach((id) => {

            const userWs = virtualUsers.get(id);//virtual users

            if (id.includes("Second")) {
                userWs.send(JSON.stringify({ type: "smessage", sCode: message.type, coord: message.coord, userID: message.userID }));
            }

        });
    }

    if (message.type.includes("Second")) {

        const userIds = Array.from(virtualUsers.keys());

        userIds.forEach((id) => {

            const userWs = virtualUsers.get(id);

            if (id.includes("First")) {
                userWs.send(JSON.stringify({ type: "smessage", sCode: message.type, coord: message.coord, userID: message.userID }));
            }

        });
    }
}






