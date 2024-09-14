
//Variable Initilization
var svgWidth = 0
var svgHeight = 0;

let container = document.querySelector('.container');
let leftSection = document.querySelector('.section-left');
let centerSection = document.querySelector('.section-center');
let rightSection = document.querySelector('.section-right');
let lastImageUrl = "";
let currentClickListener, currentMouseMoveListener, currentTouchMoveListener, currentMouseDownListener, currentTouchStartListener, currentContextMenuListener;
var gtempValue = 0.0;
var gKey = '';

var margin = { top: 0, right: 0, bottom: 0, left: 0 };

//Clearing data functions

function clear(hideSections = false) {

  // Clear the contents of each section
  if (leftSection) leftSection.innerHTML = '';
  if (centerSection) centerSection.innerHTML = '';
  if (rightSection) rightSection.innerHTML = '';

  if (currentMouseMoveListener != null) {
    container.removeEventListener('mousemove', currentMouseMoveListener);
    container.removeEventListener('touchmove', currentTouchMoveListener);
    container.removeEventListener('mousedown', currentMouseDownListener);
    container.removeEventListener('touchstart', currentTouchStartListener);
    container.removeEventListener('contextmenu', currentContextMenuListener);
    currentMouseMoveListener = null;
    currentTouchMoveListener = null;
    currentMouseDownListener = null;
    currentTouchStartListener = null;
    currentContextMenuListener = null;
  }


  if (hideSections) {
    // If hideSections is true, hide the sections
    if (leftSection) leftSection.style.display = 'none';
    if (centerSection) centerSection.style.display = 'none';
    if (rightSection) rightSection.style.display = 'none';
  } else {
    // If hideSections is false, show the sections
    if (leftSection) leftSection.style.display = ''; 
    if (centerSection) centerSection.style.display = '';
    if (rightSection) rightSection.style.display = '';

  }
}
//Bounding box, indicator circle, and interactive text
function removeElements() {

  gtempValue = 0.0;
  gKey = '';

  // Select all SVGs
  const svgs = document.querySelectorAll('svg');

  svgs.forEach(svg => {
    // Find and remove the indicator circle within this specific SVG
    const indicatorCircle = svg.querySelector('.indicator-circle');
    if (indicatorCircle) {
      indicatorCircle.remove();
    }

    // Find and hide or remove the display-text within this specific SVG
    const displayText = svg.querySelector('.interactive-text');
    if (displayText) {
      displayText.remove();
    }
  });

  const existingBBox = document.getElementById('boundingBox');
  if (existingBBox) {
 
    container.removeEventListener('click', currentClickListener);
    currentClickListener = null;  // Clear the stored reference
    existingBBox.remove();
  }

}

//Initial Setup

function setup(message) {

  removeElements();

  if (message.qindex[0] === 'S') {
    const handleClick = event => handleContainerClick(event, message.qindex);
    container.addEventListener('click', handleClick);
    currentClickListener = handleClick;
  }

  if (message.qindex[0] === 'Q') {
    if (container) {
      container.style.backgroundImage = 'none';
    }
  }

}

//Logging Interaction Data

function logInteractionToLocalStorage(taskId, eventType, details) {
  const currentLogs = JSON.parse(localStorage.getItem('interactionLogs') || '[]');
  currentLogs.push({
    taskId: taskId,
    timestamp: new Date().toISOString(),
    eventType,
    details
  });
  localStorage.setItem('interactionLogs', JSON.stringify(currentLogs));
}


//Weather Chart Data Generation Code Start

function leftPanelData(panelid, data, city, year, tempKey) {
  var panel = createSubPanelLineCharts(panelid, data, city, year, tempKey);
  leftSection.appendChild(panel);

}

function centerPanelData(panelid, data, city, year, tempKey) {
  var panel = createSubPanelLineCharts(panelid, data, city, year, tempKey);
  centerSection.appendChild(panel);

}

function rightPanelData(panelid, data, city, year, tempKey) {
  var panel = createSubPanelLineCharts(panelid, data, city, year, tempKey);
  rightSection.appendChild(panel);
}


async function filterData(filename) {

  try {
    return d3.csv(filename, (d) => {

      // Get the column names
      const date = new Date(d3.timeParse("%Y-%m-%d")(d.Date));

      // Extract the year from the Date object
      const yearInData = date.getFullYear();
      // Format the data in the CSV file
      return {
        date: date, valueMax: d.valueMax, valueMin: d.valueMin, year: yearInData
      };
    }).then((data) => {
      // Filter the data based on a custom filter function
      const filteredData = data.filter((d) => {
        return d;

      });

      // Return the filtered data
      return filteredData;
    });
  }
  catch (error) {
    console.error('Error loading data:', error);
    throw error;
  }
}

//Weather Q&A Question
async function createQuestionData(messageString) {

  try {

    clear();

    if (messageString == "Q1") {

      // Left Panel Data
      let filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2001_P1D.csv');
      leftPanelData("panel-1", filteredData, "HALIFAX", 2001, "MAX");

      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2002_P1D.csv');
      leftPanelData("panel-2", filteredData, "HALIFAX", 2002, "MAX");

      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2003_P1D.csv');
      leftPanelData("panel-3", filteredData, "HALIFAX", 2003, "MAX");

      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2004_P1D.csv');
      leftPanelData("panel-4", filteredData, "HALIFAX", 2004, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2005_P1D.csv');
      centerPanelData("panel-5", filteredData, "HALIFAX", 2005, "MAX");

      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2006_P1D.csv');
      centerPanelData("panel-6", filteredData, "HALIFAX", 2006, "MAX");

      // Right Panel Data
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2001_P1D.csv');
      rightPanelData("panel-9", filteredData, "CALGARY", 2001, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2002_P1D.csv');
      rightPanelData("panel-10", filteredData, "CALGARY", 2002, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2003_P1D.csv');
      rightPanelData("panel-11", filteredData, "CALGARY", 2003, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2004_P1D.csv');
      rightPanelData("panel-12", filteredData, "CALGARY", 2004, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2005_P1D.csv');
      centerPanelData("panel-7", filteredData, "CALGARY", 2005, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2006_P1D.csv');
      centerPanelData("panel-8", filteredData, "CALGARY", 2006, "MAX");

    }
    if (messageString == "Q2") {


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2007_P1D.csv');
      leftPanelData("panel-1", filteredData, "HALIFAX", 2007, "MAX");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2008_P1D.csv');
      leftPanelData("panel-2", filteredData, "HALIFAX", 2008, "MAX");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2009_P1D.csv');
      leftPanelData("panel-3", filteredData, "HALIFAX", 2009, "MAX");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2010_P1D.csv');
      leftPanelData("panel-4", filteredData, "HALIFAX", 2010, "MAX");


      //Center

      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2011_P1D.csv');
      centerPanelData("panel-5", filteredData, "HALIFAX", 2011, "MAX");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2012_P1D.csv');
      centerPanelData("panel-6", filteredData, "HALIFAX", 2012, "MAX");


      //Right

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2007_P1D.csv');
      rightPanelData("panel-9", filteredData, "CALGARY", 2007, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2008_P1D.csv');
      rightPanelData("panel-10", filteredData, "CALGARY", 2008, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2009_P1D.csv');
      rightPanelData("panel-11", filteredData, "CALGARY", 2009, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2010_P1D.csv');
      rightPanelData("panel-12", filteredData, "CALGARY", 2010, "MAX");


      //Center

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2011_P1D.csv');
      centerPanelData("panel-7", filteredData, "CALGARY", 2011, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2012_P1D.csv');
      centerPanelData("panel-8", filteredData, "CALGARY", 2012, "MAX");


    }
    if (messageString == "Q3") {
      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2001_P1D.csv');
      leftPanelData("panel-1", filteredData, "HALIFAX", 2001, "MIN");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2002_P1D.csv');
      leftPanelData("panel-2", filteredData, "HALIFAX", 2002, "MIN");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2003_P1D.csv');
      leftPanelData("panel-3", filteredData, "HALIFAX", 2003, "MIN");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2004_P1D.csv');
      leftPanelData("panel-4", filteredData, "HALIFAX", 2004, "MIN");



      //Center
      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2005_P1D.csv');
      centerPanelData("panel-5", filteredData, "HALIFAX", 2005, "MIN");


      filteredData = await filterData('data/cities/Halifax/en_climate_daily_NS_8202250_2006_P1D.csv');
      centerPanelData("panel-6", filteredData, "HALIFAX", 2006, "MIN");



      //Right
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2001_P1D.csv');
      rightPanelData("panel-9", filteredData, "CALGARY", 2001, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2002_P1D.csv');
      rightPanelData("panel-10", filteredData, "CALGARY", 2002, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2003_P1D.csv');
      rightPanelData("panel-11", filteredData, "CALGARY", 2003, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2004_P1D.csv');
      rightPanelData("panel-12", filteredData, "CALGARY", 2004, "MIN");


      //Center
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2005_P1D.csv');
      centerPanelData("panel-7", filteredData, "CALGARY", 2005, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2006_P1D.csv');
      centerPanelData("panel-8", filteredData, "CALGARY", 2006, "MIN");


    }
    if (messageString == "Q4") {

      // Left Panel Data
      let filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2001_P1D.csv');
      leftPanelData("panel-1", filteredData, "MONTREAL", 2001, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2002_P1D.csv');
      leftPanelData("panel-2", filteredData, "MONTREAL", 2002, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2003_P1D.csv');
      leftPanelData("panel-3", filteredData, "MONTREAL", 2003, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2004_P1D.csv');
      leftPanelData("panel-4", filteredData, "MONTREAL", 2004, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2005_P1D.csv');
      centerPanelData("panel-5", filteredData, "MONTREAL", 2005, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2006_P1D.csv');
      centerPanelData("panel-6", filteredData, "MONTREAL", 2006, "MAX");

      // Right Panel Data
      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2001_P1D.csv');
      rightPanelData("panel-9", filteredData, "Ottawa", 2001, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2002_P1D.csv');
      rightPanelData("panel-10", filteredData, "Ottawa", 2002, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2003_P1D.csv');
      rightPanelData("panel-11", filteredData, "Ottawa", 2003, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2004_P1D.csv');
      rightPanelData("panel-12", filteredData, "Ottawa", 2004, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2005_P1D.csv');
      centerPanelData("panel-7", filteredData, "Ottawa", 2005, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2006_P1D.csv');
      centerPanelData("panel-8", filteredData, "Ottawa", 2006, "MAX");

    }
    if (messageString == "Q5") {


      // Left Panel Data
      let filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2007_P1D.csv');
      leftPanelData("panel-1", filteredData, "Montreal", 2007, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2008_P1D.csv');
      leftPanelData("panel-2", filteredData, "Montreal", 2008, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2009_P1D.csv');
      leftPanelData("panel-3", filteredData, "Montreal", 2009, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2010_P1D.csv');
      leftPanelData("panel-4", filteredData, "Montreal", 2010, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2011_P1D.csv');
      centerPanelData("panel-5", filteredData, "Montreal", 2011, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2012_P1D.csv');
      centerPanelData("panel-6", filteredData, "Montreal", 2012, "MAX");

      // Right Panel Data
      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2007_P1D.csv');
      rightPanelData("panel-9", filteredData, "Ottawa", 2007, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2008_P1D.csv');
      rightPanelData("panel-10", filteredData, "Ottawa", 2008, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2009_P1D.csv');
      rightPanelData("panel-11", filteredData, "Ottawa", 2009, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2010_P1D.csv');
      rightPanelData("panel-12", filteredData, "Ottawa", 2010, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2011_P1D.csv');
      centerPanelData("panel-7", filteredData, "Ottawa", 2011, "MAX");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106001_2012_P1D.csv');
      centerPanelData("panel-8", filteredData, "Ottawa", 2012, "MAX");


    }
    if (messageString == "Q6") {
      // Left Panel Data
      let filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2001_P1D.csv');
      leftPanelData("panel-1", filteredData, "Montreal", 2001, "MIN");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2002_P1D.csv');
      leftPanelData("panel-2", filteredData, "Montreal", 2002, "MIN");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2003_P1D.csv');
      leftPanelData("panel-3", filteredData, "Montreal", 2003, "MIN");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2004_P1D.csv');
      leftPanelData("panel-4", filteredData, "Montreal", 2004, "MIN");

      // Center Panel Data
      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2005_P1D.csv');
      centerPanelData("panel-5", filteredData, "Montreal", 2005, "MIN");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2006_P1D.csv');
      centerPanelData("panel-6", filteredData, "Montreal", 2006, "MIN");

      // Right Panel Data
      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2001_P1D.csv');
      rightPanelData("panel-9", filteredData, "Ottawa", 2001, "MIN");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2002_P1D.csv');
      rightPanelData("panel-10", filteredData, "Ottawa", 2002, "MIN");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2003_P1D.csv');
      rightPanelData("panel-11", filteredData, "Ottawa", 2003, "MIN");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2004_P1D.csv');
      rightPanelData("panel-12", filteredData, "Ottawa", 2004, "MIN");

      // Center Panel Data
      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2005_P1D.csv');
      centerPanelData("panel-7", filteredData, "Ottawa", 2005, "MIN");

      filteredData = await filterData('data/cities/Ottawa/en_climate_daily_ON_6106000_2006_P1D.csv');
      centerPanelData("panel-8", filteredData, "Ottawa", 2006, "MIN");


    }
    if (messageString == "Q7") {

      // Left Panel Data
      let filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2011_P1D.csv');
      leftPanelData("panel-1", filteredData, "Toronto", 2011, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2012_P1D.csv');
      leftPanelData("panel-2", filteredData, "Toronto", 2012, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2013_P1D.csv');
      leftPanelData("panel-3", filteredData, "Toronto", 2013, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158731_2014_P1D.csv');
      leftPanelData("panel-4", filteredData, "Toronto", 2014, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158731_2015_P1D.csv');
      centerPanelData("panel-5", filteredData, "Toronto", 2015, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158731_2016_P1D.csv');
      centerPanelData("panel-6", filteredData, "Toronto", 2016, "MAX");

      // Right Panel Data
      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2011_P1D.csv');
      rightPanelData("panel-9", filteredData, "Vancouver", 2011, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2012_P1D.csv');
      rightPanelData("panel-10", filteredData, "Vancouver", 2012, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2013_P1D.csv');
      rightPanelData("panel-11", filteredData, "Vancouver", 2013, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108395_2014_P1D.csv');
      rightPanelData("panel-12", filteredData, "Vancouver", 2014, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108395_2015_P1D.csv');
      centerPanelData("panel-7", filteredData, "Vancouver", 2015, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108395_2016_P1D.csv');
      centerPanelData("panel-8", filteredData, "Vancouver", 2016, "MAX");

    }
    if (messageString == "Q8") {


      // Left Panel Data
      let filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2007_P1D.csv');
      leftPanelData("panel-1", filteredData, "Toronto", 2007, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2008_P1D.csv');
      leftPanelData("panel-2", filteredData, "Toronto", 2008, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2009_P1D.csv');
      leftPanelData("panel-3", filteredData, "Toronto", 2009, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2010_P1D.csv');
      leftPanelData("panel-4", filteredData, "Toronto", 2010, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2011_P1D.csv');
      centerPanelData("panel-5", filteredData, "Toronto", 2011, "MAX");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2012_P1D.csv');
      centerPanelData("panel-6", filteredData, "Toronto", 2012, "MAX");

      // Right Panel Data
      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2007_P1D.csv');
      rightPanelData("panel-9", filteredData, "Vancouver", 2007, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2008_P1D.csv');
      rightPanelData("panel-10", filteredData, "Vancouver", 2008, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2009_P1D.csv');
      rightPanelData("panel-11", filteredData, "Vancouver", 2009, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2010_P1D.csv');
      rightPanelData("panel-12", filteredData, "Vancouver", 2010, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2011_P1D.csv');
      centerPanelData("panel-7", filteredData, "Vancouver", 2011, "MAX");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2012_P1D.csv');
      centerPanelData("panel-8", filteredData, "Vancouver", 2012, "MAX");


    }
    if (messageString == "Q9") {
      // Left Panel Data
      let filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2005_P1D.csv');
      leftPanelData("panel-1", filteredData, "Toronto", 2005, "MIN");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2006_P1D.csv');
      leftPanelData("panel-2", filteredData, "Toronto", 2006, "MIN");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2007_P1D.csv');
      leftPanelData("panel-3", filteredData, "Toronto", 2007, "MIN");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2008_P1D.csv');
      leftPanelData("panel-4", filteredData, "Toronto", 2008, "MIN");

      // Center Panel Data
      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2009_P1D.csv');
      centerPanelData("panel-5", filteredData, "Toronto", 2009, "MIN");

      filteredData = await filterData('data/cities/Toronto/en_climate_daily_ON_6158733_2010_P1D.csv');
      centerPanelData("panel-6", filteredData, "Toronto", 2010, "MIN");

      // Right Panel Data
      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2005_P1D.csv');
      rightPanelData("panel-9", filteredData, "Vancouver", 2005, "MIN");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2006_P1D.csv');
      rightPanelData("panel-10", filteredData, "Vancouver", 2006, "MIN");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2007_P1D.csv');
      rightPanelData("panel-11", filteredData, "Vancouver", 2007, "MIN");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2008_P1D.csv');
      rightPanelData("panel-12", filteredData, "Vancouver", 2008, "MIN");

      // Center Panel Data
      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2009_P1D.csv');
      centerPanelData("panel-7", filteredData, "Vancouver", 2009, "MIN");

      filteredData = await filterData('data/cities/Vancouver/en_climate_daily_BC_1108447_2010_P1D.csv');
      centerPanelData("panel-8", filteredData, "Vancouver", 2010, "MIN");


    }
    if (messageString == "Q10") {

      // Left Panel Data
      let filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2005_P1D.csv');
      leftPanelData("panel-1", filteredData, "Montreal", 2005, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2006_P1D.csv');
      leftPanelData("panel-2", filteredData, "Montreal", 2006, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2007_P1D.csv');
      leftPanelData("panel-3", filteredData, "Montreal", 2007, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2008_P1D.csv');
      leftPanelData("panel-4", filteredData, "Montreal", 2008, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2009_P1D.csv');
      centerPanelData("panel-5", filteredData, "Montreal", 2009, "MAX");

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2010_P1D.csv');
      centerPanelData("panel-6", filteredData, "Montreal", 2010, "MAX");

      // Right Panel Data
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2005_P1D.csv');
      rightPanelData("panel-9", filteredData, "CALGARY", 2005, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2006_P1D.csv');
      rightPanelData("panel-10", filteredData, "CALGARY", 2006, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2007_P1D.csv');
      rightPanelData("panel-11", filteredData, "CALGARY", 2007, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2008_P1D.csv');
      rightPanelData("panel-12", filteredData, "CALGARY", 2008, "MAX");

      // Center Panel Data
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2009_P1D.csv');
      centerPanelData("panel-7", filteredData, "CALGARY", 2009, "MAX");

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2010_P1D.csv');
      centerPanelData("panel-8", filteredData, "CALGARY", 2010, "MAX");

    }
    if (messageString == "Q11") {


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2011_P1D.csv');
      leftPanelData("panel-1", filteredData, "Montreal", 2011, "MAX");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_7025250_2012_P1D.csv');
      leftPanelData("panel-2", filteredData, "Montreal", 2012, "MAX");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2013_P1D.csv');
      leftPanelData("panel-3", filteredData, "Montreal", 2013, "MAX");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2014_P1D.csv');
      leftPanelData("panel-4", filteredData, "Montreal", 2014, "MAX");


      //Center

      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2015_P1D.csv');
      centerPanelData("panel-5", filteredData, "Montreal", 2015, "MAX");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2016_P1D.csv');
      centerPanelData("panel-6", filteredData, "Montreal", 2016, "MAX");


      //Right

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2011_P1D.csv');
      rightPanelData("panel-9", filteredData, "CALGARY", 2011, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031093_2012_P1D.csv');
      rightPanelData("panel-10", filteredData, "CALGARY", 2012, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2013_P1D.csv');
      rightPanelData("panel-11", filteredData, "CALGARY", 2013, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2014_P1D.csv');
      rightPanelData("panel-12", filteredData, "CALGARY", 2014, "MAX");


      //Center

      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2015_P1D.csv');
      centerPanelData("panel-7", filteredData, "CALGARY", 2015, "MAX");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2016_P1D.csv');
      centerPanelData("panel-8", filteredData, "CALGARY", 2016, "MAX");


    }
    if (messageString == "Q12") {
      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2017_P1D.csv');
      leftPanelData("panel-1", filteredData, "Montreal", 2017, "MAX");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2018_P1D.csv');
      leftPanelData("panel-2", filteredData, "Montreal", 2018, "MIN");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2019_P1D.csv');
      leftPanelData("panel-3", filteredData, "Montreal", 2019, "MIN");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2020_P1D.csv');
      leftPanelData("panel-4", filteredData, "Montreal", 2020, "MIN");



      //Center
      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2021_P1D.csv');
      centerPanelData("panel-5", filteredData, "Montreal", 2021, "MIN");


      filteredData = await filterData('data/cities/Montreal/en_climate_daily_QC_702S006_2022_P1D.csv');
      centerPanelData("panel-6", filteredData, "Montreal", 2022, "MIN");



      //Right
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2017_P1D.csv');
      rightPanelData("panel-9", filteredData, "CALGARY", 2017, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2018_P1D.csv');
      rightPanelData("panel-10", filteredData, "CALGARY", 2018, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2019_P1D.csv');
      rightPanelData("panel-11", filteredData, "CALGARY", 2019, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2020_P1D.csv');
      rightPanelData("panel-12", filteredData, "CALGARY", 2020, "MIN");


      //Center
      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2021_P1D.csv');
      centerPanelData("panel-7", filteredData, "CALGARY", 2021, "MIN");


      filteredData = await filterData('data/cities/Calgary/en_climate_daily_AB_3031094_2022_P1D.csv');
      centerPanelData("panel-8", filteredData, "CALGARY", 2022, "MIN");


    }
    setupEventDelegation(messageString);

    return true;

  } catch (error) {
    console.error('Failed to create question data:', error);
  }

}

//Create 3 sections and 12 panels
function create(svgWidth, svgHeight) {
  clear();

  for (let i = 1; i <= 4; i++) {

    var id = `panel-${i}`;
    var panel = createSubPanel(id, svgWidth, svgHeight);
    leftSection.appendChild(panel);

  }


  //center panel
  for (let i = 5; i <= 8; i++) {
    var id = `panel-${i}`;
    var panel = createSubPanel(id, svgWidth, svgHeight);
    centerSection.appendChild(panel);
  }


  //right panel
  for (let i = 9; i <= 12; i++) {

    var id = `panel-${i}`;
    var panel = createSubPanel(id, svgWidth, svgHeight);
    rightSection.appendChild(panel);
  }

}

//Initial
function createSubPanel(id, Width, Height, bDraw = false) {

  svgWidth = Width;
  svgHeight = Height;

  var stroke_width, chart_title_x, chart_title_y, x_axis_m, y_axis_m, legend_line_x1, legend_line_x2, legend_line_y1, legend_line_y2, max_min_x, max_min_y = 0;

  if (svgWidth > 1000) {
    stroke_width = 4;
    margin = { top: 20, right: 40, bottom: 80, left: 120 };
    chart_title_x = svgWidth / 2;
    chart_title_y = 80;
    x_axis_m = 10;
    y_axis_m = 60;
    legend_line_x1 = 0;
    legend_line_x2 = 100;
    max_min_x = -100;
    max_min_y = 20;
    legend_line_y1 = 15;
    legend_line_y2 = 15;

  }
  else {
    stroke_width = 2;
    margin = { top: 20, right: 40, bottom: 60, left: 80 };
    chart_title_x = svgWidth / 2;;
    chart_title_y = 40;
    x_axis_m = 5;
    y_axis_m = 40;
    legend_line_x1 = 40;
    legend_line_x2 = 100;
    max_min_x = 1;
    max_min_y = 10;
    legend_line_y1 = 5;
    legend_line_y2 = 5;

  }


  var div = document.createElement("div");
  div.setAttribute("id", id);
  div.classList.add("sub-panel");

  var svg = d3.select(div)
    .append("svg");


  svg.attr("width", svgWidth)
    .attr("height", svgHeight);

  // Define margins for chart
  var width = svgWidth - margin.left - margin.right;
  var height = svgHeight - margin.top - margin.bottom;

  // Define data for line chart
  var data = [
    { x: 0, y: 5 },
    { x: 50, y: 20 },
    { x: 100, y: 10 },
    { x: 150, y: 25 },
    { x: 200, y: 15 },
    { x: 250, y: 30 },
    { x: 300, y: 20 }
  ];

  // Define scales and axis for line chart
  var xScale = d3.scaleLinear()
    .domain([0, 300])
    .range([0, width]);
  var yScale = d3.scaleLinear()
    .domain([0, 30])
    .range([height, 0]);


  var xAxis = d3.axisBottom(xScale);
  var yAxis = d3.axisLeft(yScale);

  // Add line to SVG
  var chart = svg.append("g")
    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");
  chart.append("path")
    .datum(data)
    .attr("fill", "none")
    .attr("stroke", "orange")
    .attr("stroke-width", stroke_width)
    .attr("d", d3.line()
      .x(function (d) { return xScale(d.x); })
      .y(function (d) { return yScale(d.y); })
    );

  // Add X and Y axis to chart
  chart.append("g")
    .attr("class", "axis x-axis")
    .attr("transform", "translate(0," + height + ")")
    .call(xAxis);
  chart.append("g")
    .attr("class", "axis y-axis")
    .call(yAxis);

  // Add chart title
  svg.append("text")
    .attr("class", "chart-title")
    .attr("x", chart_title_x)
    .attr("y", chart_title_y)
    .text("Title");


  // Add x-axis label
  svg.append("text")
    .attr("class", "x axis-label")
    .attr("text-anchor", "middle")
    .attr("x", width / 2 + margin.left)
    .attr("y", height + margin.bottom + x_axis_m)
    .text("X-Axis Label");

  // Add y-axis label
  svg.append("text")
    .attr("class", "y axis-label")
    .attr("text-anchor", "middle")
    .attr("x", -height / 2)
    .attr("y", margin.left - y_axis_m)
    .attr("transform", 'rotate(-90)')
    .text("Y-Axis Label");


  const legend = svg.append('g')
    .attr('class', 'legend')
    .attr('transform', `translate(${width},${50})`);


  legend.append('text')
    .attr('x', max_min_x)
    .attr('y', max_min_y)
    .text('Legend');

  legend.append('line')
    .attr('class', 'legend-line')
    .attr('x1', legend_line_x1)
    .attr('y1', legend_line_y1)
    .attr('x2', legend_line_x2)
    .attr('y2', legend_line_y2);

  return div;
}


//Dynamic line charts
function createSubPanelLineCharts(id, data, city, year, tempKey, bDraw = false) {

  gKey = tempKey;

  var stroke_width, chart_title_x, chart_title_y, x_axis_m, y_axis_m, legend_line_x1, legend_line_x2, legend_line_y1, legend_line_y2, max_min_x, max_min_y = 0;

  if (svgWidth > 1000) {
    stroke_width = 4;
    margin = { top: 20, right: 40, bottom: 80, left: 120 };
    chart_title_x = svgWidth / 2;
    chart_title_y = 80;
    x_axis_m = 10;
    y_axis_m = 60;

  }
  else {
    stroke_width = 2;
    margin = { top: 20, right: 40, bottom: 60, left: 80 };
    chart_title_x = svgWidth / 2;;
    chart_title_y = 40;
    x_axis_m = 5;
    y_axis_m = 40;

  }


  var div = document.createElement("div");
  div.setAttribute("id", id);
  div.classList.add("sub-panel");

  var svg = d3.select(div)
    .append("svg");

  // Set dimensions for SVG
  svg.attr("width", svgWidth)
    .attr("height", svgHeight);

  // Define margins for chart
  var width = svgWidth - margin.left - margin.right;
  var height = svgHeight - margin.top - margin.bottom;

  // Define scales and axis for line chart
  var xScale = d3.scaleTime()
    .domain(d3.extent(data, function (d) { return d.date; }))
    .range([0, width]);

  // Modify yScale to have a constant domain from -50 - constadd to 50 + constadd
  var yScale = d3.scaleLinear()
    .domain([-50, 50])
    .range([height, 0]);

  // Add line to SVG
  var chart = svg.append("g")
    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

  if (tempKey == "MAX") {
    chart.append("path")
      .datum(data)
      .attr("fill", "none")
      .attr("stroke", "orange")
      .attr("stroke-width", stroke_width)
      .attr("vector-effect", "non-scaling-stroke")
      .attr("d", d3.line()
        .x(function (d) { return xScale(d.date); })
        .y(function (d) { return yScale(+d.valueMax); })
        .curve(d3.curveLinear)
      ).style("shape-rendering", "crispEdges");


    // Append invisible circles for each data point
    chart.selectAll(".data-point")
      .data(data)
      .enter().append("circle")
      .attr("class", "data-point")
      .attr("cx", d => xScale(d.date))
      .attr("cy", d => yScale(+d.valueMax))
      .attr("r", 5)
      .attr("fill", "none")
      .attr("stroke", "none")
      .attr("pointer-events", "all");

  }
  if (tempKey == "MIN") {
    chart.append("path")
      .datum(data)
      .attr("fill", "none")
      .attr("stroke", "orange")
      .attr("stroke-width", stroke_width)
      .attr("vector-effect", "non-scaling-stroke")
      .attr("d", d3.line()
        .x(function (d) { return xScale(d.date); })
        .y(function (d) { return yScale(+d.valueMin); })
        .curve(d3.curveLinear)
      ).style("shape-rendering", "crispEdges");

    // Append invisible circles for each data point
    chart.selectAll(".data-point")
      .data(data)
      .enter().append("circle")
      .attr("class", "data-point")
      .attr("cx", d => xScale(d.date))
      .attr("cy", d => yScale(+d.valueMin))
      .attr("r", 5)
      .attr("fill", "none")
      .attr("stroke", "none")
      .attr("pointer-events", "all");
  }


  var xAxis = d3.axisBottom(xScale)
    .tickFormat(d3.timeFormat("%b"));

  var yAxis = d3.axisLeft(yScale);


  // Add X and Y axis to chart
  chart.append("g")
    .attr("class", "axis x-axis")
    .attr("transform", "translate(0," + height + ")")
    .call(xAxis);
  chart.append("g")
    .attr("class", "axis y-axis")
    .call(yAxis);


  // Add chart title
  svg.append("text")
    .attr("class", "chart-title")
    .attr("x", chart_title_x)
    .attr("y", chart_title_y)
    .text(city + " (" + year + ")");


  // Add x-axis label
  svg.append("text")
    .attr("class", "x axis-label")
    .attr("text-anchor", "middle")
    .attr("x", width / 2 + margin.left)
    .attr("y", height + margin.bottom + x_axis_m)
    .text("Month");

  // Add y-axis label
  svg.append("text")
    .attr("class", "y axis-label")
    .attr("text-anchor", "middle")
    .attr("x", -height / 2)
    .attr("y", margin.left - y_axis_m)
    .attr("transform", 'rotate(-90)')
    .text("Temperature (C)");
    

  return div;
}

function swapRowsAcrossSections() {
  const sections = ['.section-left', '.section-center', '.section-right'];

  sections.forEach(sectionSelector => {
    const section = document.querySelector(sectionSelector);
    const panels = section.querySelectorAll('.sub-panel');

    if (panels.length >= 4) {
      // Temporarily store the top row panels
      const topRowPanels = [panels[0], panels[1]];

      // Temporarily remove top row panels from the DOM to avoid reflow issues
      const parent = panels[0].parentNode;
      parent.removeChild(panels[0]);
      parent.removeChild(panels[1]);

      // Move bottom row panels to the top row
      parent.insertBefore(panels[2], parent.firstChild);
      parent.insertBefore(panels[3], parent.children[1]); // now it becomes the second child

      // Move the originally top row panels to the bottom row
      parent.appendChild(topRowPanels[0]);
      parent.appendChild(topRowPanels[1]);
    }
  });
}



//Weather Chart Data Interactivity Functions Start
function setupEventDelegation(messageString) {

  const MouseMoveListener = event => (handleMove(event, messageString), false);
  const TouchMoveListener = event => (handleMove(event, messageString), { passive: false });
  const MouseDownListener = event => (event.preventDefault(), false);
  const TouchStartListener = event => (event.preventDefault(), false);
  const ContextMenuListener = event => (event.preventDefault());

  container.addEventListener('mousemove', MouseMoveListener);
  container.addEventListener('touchmove', TouchMoveListener);
  container.addEventListener('mousedown', MouseDownListener);
  container.addEventListener('touchstart', TouchStartListener);
  container.addEventListener('contextmenu', ContextMenuListener);

  currentMouseMoveListener = MouseMoveListener;
  currentTouchMoveListener = TouchMoveListener;
  currentMouseDownListener = MouseDownListener;
  currentTouchStartListener = TouchStartListener;
  currentContextMenuListener = ContextMenuListener;


  function handleMove(event, taskId) {
    event.preventDefault();
    const pointers = event.touches ? event.touches : [event];
    Array.from(pointers).forEach(pointer => {
      let target = document.elementFromPoint(pointer.clientX, pointer.clientY);
      let svg = target ? target.closest('svg') : null;

      if (!svg) return;

      const svgRect = svg.getBoundingClientRect();
      const pointerX = pointer.clientX - svgRect.left;
      const pointerY = pointer.clientY - svgRect.top;


      updateIndicator(svg, pointerX, pointerY);
      displayTemperature(svg, pointerX, pointerY, taskId, svgRect);
    });
  }
}

function updateIndicator(svg, pointerX, pointerY) {
  let indicatorCircle = svg.querySelector('.indicator-circle');
  if (!indicatorCircle) {
    indicatorCircle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    indicatorCircle.setAttribute('class', 'indicator-circle');
    indicatorCircle.setAttribute('r', 30);
    indicatorCircle.setAttribute('fill', 'none');
    indicatorCircle.setAttribute('stroke', 'red');
    svg.appendChild(indicatorCircle);
  }
  indicatorCircle.setAttribute('cx', pointerX);
  indicatorCircle.setAttribute('cy', pointerY);
  indicatorCircle.style.visibility = 'visible';
}




function displayTemperature(svg, pointerX, pointerY, taskId, svgRect) {
  var radius = 30;
  var closestData = findClosestData(pointerX - margin.left, pointerY - margin.top, radius, svg);
  var interactiveText = svg.querySelector('.interactive-text');
  if (!interactiveText) {
    interactiveText = document.createElementNS("http://www.w3.org/2000/svg", "text");
    interactiveText.setAttribute('class', 'interactive-text');
    interactiveText.setAttribute('fill', 'white');
    interactiveText.setAttribute('font-size', '16px');
    interactiveText.setAttribute('font-weight', 'bold');
    svg.appendChild(interactiveText);
  }


  if (closestData) {

    var tempKey = gKey === "MAX" ? 'valueMax' : 'valueMin';
    var tempValue = Number(closestData[tempKey]);
    var formattedDate = d3.timeFormat("%B %d, %Y")(closestData.date);

    if (!isNaN(tempValue)) {
    
      interactiveText.innerHTML = `Date: ${formattedDate} Temp: ${tempValue.toFixed(2)}°C`;

      // Measure text element size
      let bbox = interactiveText.getBBox();

      // Adjust x and y to ensure the text stays within the bounds of the container
      if (pointerX + bbox.width > svgRect.width) pointerX = svgRect.width - bbox.width;
      if (pointerX < 0) pointerX = 0;
      if (pointerY - bbox.height < 0) pointerY = bbox.height;
      if (pointerY > svgRect.height) pointerY = svgRect.height;

      interactiveText.setAttribute('x', pointerX);
      interactiveText.setAttribute('y', pointerY - 15);
      interactiveText.setAttribute('text-anchor', 'middle');
      interactiveText.style.display = 'block';


      if (tempValue != gtempValue) {
        logInteractionToLocalStorage(taskId, 'Temperature Displayed', {
          date: formattedDate,
          temperature: tempValue,
          position: { x: pointerX, y: pointerY }
        });
        gtempValue = tempValue;
      }



      const tempText = `Date: ${formattedDate} Temp: ${tempValue.toFixed(2)}°C`;
      const titleElement = svg.querySelector('.chart-title');
      const svgTitle = titleElement ? titleElement.textContent : 'Unknown SVG';

      const oldWidth = 1918;
      const oldHeight = 1078;
      const newWidth = 959;
      const newHeight = 539;

      // Calculate new coordinates based on the scaling
      let newX = Math.round(pointerX * (newWidth / oldWidth));
      let newY = Math.round(pointerY * (newHeight / oldHeight));


      sendMessageDynamic({ type: 'temp', svgTitle: svgTitle, text: tempText, X: newX, Y: newY });

    } else {
      interactiveText.style.display = 'none';
    }
  } else {
    interactiveText.style.display = 'none';
  }
}

function findClosestData(mouseX, mouseY, radius, svg) {
  var closestData = null;
  var minDistance = Infinity;
  var dataPoints = svg.querySelectorAll('.data-point');


  dataPoints.forEach(point => {
 
    var x = parseFloat(point.getAttribute('cx'));
    var y = parseFloat(point.getAttribute('cy'));

    var distance = Math.sqrt(Math.pow(x - mouseX, 2) + Math.pow(y - mouseY, 2));
    if (distance < minDistance && distance <= radius) {
      closestData = point.__data__;
      minDistance = distance;
    }
  });
  return closestData;
}

//Weather Chart Data Interactivity Functions End

//Weather Chart Data Generation Code End


//Search and Dashbarod Functions Start

function createImageSearchLargeDisplay(message) {

  
  // Ensure the initial bounding box is created, if not already
  if (!document.getElementById('boundingBox')) {
    createInitialBoundingBox(container);
  }

  const imageUrl = message.image;

  // Only update the image if the imageUrl is different from the last one
  if (imageUrl !== lastImageUrl) {
    clear(true);
    container.style.backgroundImage = 'none';

    // Create a new image element
    const image = new Image();
    image.src = imageUrl;

    image.onload = function () {
      container.style.backgroundImage = `url('${imageUrl}')`;
      container.style.backgroundSize = 'contain';
      container.style.backgroundPosition = 'center';
      container.style.backgroundRepeat = 'no-repeat';
      lastImageUrl = imageUrl;
    };

  } else {
    console.log("Image URL is the same as the last one. Skipping update.");
  }
}

function createImageSearchCurvedARScreen(message) {

  
  const imageUrl = message.image.replace(/(\.jpg)$/, '_ar$1');

  // Only update the image if the imageUrl is different from the last one
  if (imageUrl !== lastImageUrl) {
    clear(true);
    container.style.backgroundImage = 'none';

    // Create a new image element
    const image = new Image();
    image.src = imageUrl;

    image.onload = function () {
      container.style.backgroundImage = `url('${imageUrl}')`;
      container.style.backgroundSize = 'contain';
      container.style.backgroundPosition = 'center';
      container.style.backgroundRepeat = 'no-repeat';
      lastImageUrl = imageUrl;

    };

  } else {
    console.log("Image URL is the same as the last one. Skipping update.");
  }
}

function createImageInsight(message) {

  
  const imageUrl = message.image;

  // Only update the image if the imageUrl is different from the last one
  if (imageUrl !== lastImageUrl) {
    clear(true);
    container.style.backgroundImage = 'none';

    // Create a new image element
    const image = new Image();
    image.src = imageUrl;

    image.onload = function () {
      container.style.backgroundImage = `url('${imageUrl}')`;
      container.style.backgroundSize = 'contain';
      container.style.backgroundPosition = 'center';
      container.style.backgroundRepeat = 'no-repeat'; 
      lastImageUrl = imageUrl;

    };

  } else {
    console.log("Image URL is the same as the last one. Skipping update.");
  }
}

//Search Data Interactivity Start

function createInitialBoundingBox(container) {
  const bbox = document.createElement('div');
  bbox.setAttribute('id', 'boundingBox');
  bbox.style.position = 'absolute';
  bbox.style.width = '100px';
  bbox.style.height = '100px';
  bbox.style.backgroundColor = 'white';
  bbox.style.border = '2px solid black';
  bbox.style.color = 'black'; 
  bbox.style.display = 'none';
  container.appendChild(bbox);
}

function handleContainerClick(event, taskID) {

  
  const container = event.target;
  const bbox = document.getElementById('boundingBox');
  let x = event.offsetX; // x position within the container
  let y = event.offsetY; // y position within the container

  const bboxWidth = bbox.offsetWidth;
  const bboxHeight = bbox.offsetHeight;
  const maxX = container.offsetWidth - bboxWidth;
  const maxY = container.offsetHeight - bboxHeight;

  // Adjust if the new position is out of bounds considering the bounding box size
  if (x > maxX) x = maxX;
  if (x < 0) x = 0;
  if (y > maxY) y = maxY;
  if (y < 0) y = 0;

  // Update the bounding box position and make it visible
  bbox.style.left = `${x}px`;
  bbox.style.top = `${y}px`;
  bbox.style.display = 'flex';
  bbox.style.fontSize = '16px'; 
  bbox.style.fontWeight = 'bold';
  bbox.innerText = `X: ${x}, Y: ${y}`;

  logInteractionToLocalStorage(taskID, 'Container Click', { x, y });
  sendMessageDynamic({ type: 'bbox', X: x, Y: y });//both virtual system users websites


}

//Search Data Interactivity End

//Search and Dashbarod Functions End