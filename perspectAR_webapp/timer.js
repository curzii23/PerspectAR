

let timerInterval;

// Variables to track the timer state
let isTimerPaused = false;
let elapsedTime = 0;
let startTime;



function startTimer() {

    startTime = Date.now(); // Set the start time for the current question
    timerInterval = setInterval(updateCounter, 1000);
}

function resetTimer() {
    clearInterval(timerInterval);

    const counterElement = document.getElementById("counter");
    counterElement.textContent = "0 seconds";
}

function pauseTimer() {
    clearInterval(timerInterval);
    
}

function updateCounter() {
    
    const currentTime = Date.now();
    elapsedTime = Math.floor((currentTime - startTime) / 1000); // Calculate elapsed time in seconds


    const counterElement = document.getElementById("counter");
    counterElement.textContent = elapsedTime + " seconds";
}

function resumeTimer(questionTime)
{
    startTime = Date.now() - questionTime; // Adjust the start time by subtracting the time spent on the current question
}

        