
function QAHandle(data) {
  const questionData = JSON.parse(data.value);
  const typeQInputs = document.getElementById('typeQInputs');

  // Iterate through each question
  questionData.forEach(qData => {

    if (qData.type === 'compareCities') {

      const questionElem = createElement('div', { class: 'question', style: 'margin-bottom: 20px;' }, typeQInputs);
      createElement('p', { textContent: qData.question, style: 'margin-bottom: 10px;' }, questionElem);

      // Temperature input with reusable function
      const tempInputDiv = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempInputDiv, 'tempInput', 'Temperature:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid temperature. Example: 23.45, -23.45'
      });

      // Handling year selection
      const yearSelectDiv = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      handleYearSelect(yearSelectDiv, qData.yearRange);

      // Month selection
      const monthSelectDiv = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createSelectWithLabel(monthSelectDiv, 'monthSelect', 'Month:', [
        'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'
      ].map((month, index) => ({ value: index + 1, text: month })));
    }

    if (qData.type === 'info') {

      const questionElem = createElement('div', { class: 'question', style: 'margin-bottom: 20px;' }, typeQInputs);
      createElement('p', { textContent: qData.question, style: 'margin-bottom: 10px;' }, questionElem);
    }

  });
}

function QBHandle(data) {
  const questionsData = JSON.parse(data.value);
  const typeQInputs = document.getElementById('typeQInputs');

  questionsData.forEach(question => {

    if (question.type === 'highestTemp') {

      // Create each question block with margin for spacing
      const questionElem = createElement('div', { class: 'question', style: 'margin-bottom: 20px;' }, typeQInputs);
      createElement('p', { textContent: question.question, style: 'margin-bottom: 10px;' }, questionElem);

      // Create a container for the temperature input with margin
      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempContainer, 'tempInput', 'Temperature:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid temperature. Example: 23.45, -23.45'
      });

      // Conditionally handle year selection if yearRange exists in the question data
      if (question.yearRange) {
        const yearContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
        handleYearSelect(yearContainer, question.yearRange);
      }
    } else if (question.type === 'compareCities') {

      // Create each question block with margin for spacing
      const questionElem = createElement('div', { class: 'question', style: 'margin-bottom: 20px;' }, typeQInputs);
      createElement('p', { textContent: question.question, style: 'margin-bottom: 10px;' }, questionElem);

      // Create a container for the city select with margin
      const cityContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createSelectWithLabel(cityContainer, 'citySelect', 'City:', question.cities.map(city => ({ value: city, text: city })));

      // Create a container for the highest temperature input with margin
      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempContainer, 'tempInput', 'Temperature:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?'
      });

      if (question.yearRange) {
        const yearContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
        handleYearSelect(yearContainer, question.yearRange);
      }
    }
    else if (question.type === 'info') {

      const questionElem = createElement('div', { class: 'question', style: 'margin-bottom: 20px;' }, typeQInputs);
      createElement('p', { textContent: question.question, style: 'margin-bottom: 10px;' }, questionElem);

    }


  });
}

function QCHandle(data) {

  const questionsData = JSON.parse(data.value);
  const typeQInputs = document.getElementById('typeQInputs');

  questionsData.forEach(question => {
    // Create each question block with margin for spacing
    const questionElem = createElement('div', { class: 'question', style: 'margin-bottom: 20px;' }, typeQInputs);
    createElement('p', { textContent: question.question, style: 'margin-bottom: 10px;' }, questionElem);

    if (question.type === 'highestTemp') {
      // Create a container for the temperature input with margin
      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempContainer, 'tempInput', 'Temperature:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid temperature. Example: 23.45, -23.45'
      });

      // Conditionally handle year selection if yearRange exists in the question data
      if (question.yearRange) {
        const yearContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
        handleYearSelect(yearContainer, question.yearRange);
      }
    }
    else if (question.type === 'count') {
      // Create a container for the temperature input with margin
      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempContainer, 'countInput', 'Count:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid number.'
      });
    }
    else if (question.type === 'occurrence') {

      // Create a container for the city select with margin
      const cityContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createSelectWithLabel(cityContainer, 'citySelect', 'City:', question.cities.map(city => ({ value: city, text: city })));

      const monthSelectDiv = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createSelectWithLabel(monthSelectDiv, 'monthSelect', 'Month:', [
        'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'
      ].map((month, index) => ({ value: index + 1, text: month })));

    }
    else if (question.type === 'difference') {
      // Create a container for the temperature input with margin
      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempContainer, 'differenceInput', 'Difference:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid number.'
      });
    }
    else if (question.type === 'compareCities') {
      // Create a container for the city select with margin
      const cityContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createSelectWithLabel(cityContainer, 'citySelect', 'City:', question.cities.map(city => ({ value: city, text: city })));

      // Create a container for the highest temperature input with margin
      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionElem);
      createInputWithLabel(tempContainer, 'lowestTemp', 'Temperature:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid number.'
      });
    }
  });
}

function SAHandle(data) {
  const questionsData = JSON.parse(data.value);
  const typeSInputs = document.getElementById('typeSInputs');

  questionsData.forEach((question, index) => {
    // Create a section for each question with a unique ID
    const questionSection = document.createElement('div');
    questionSection.id = `question${index}`;
    questionSection.style.cssText = 'margin-bottom: 30px; border: 1px solid #ccc; padding: 20px;';
    typeSInputs.appendChild(questionSection);

    // Add the question text
    const questionParagraph = document.createElement('p');
    questionParagraph.textContent = question.questionText;
    questionParagraph.style.marginBottom = '20px';
    questionSection.appendChild(questionParagraph);

    // Display the target image
    const locateImage = document.createElement('img');
    locateImage.src = question.target;
    locateImage.alt = 'Target image for the task';
    locateImage.style.cssText = 'display: block; margin: 20px auto; width: auto; max-width: 100%;';
    questionSection.appendChild(locateImage);

    // Image onload adjustments
    locateImage.onload = function () {
      locateImage.width = locateImage.naturalWidth;
      locateImage.height = locateImage.naturalHeight;
    };

    // Inputs for X and Y coordinates
    const xContainer = document.createElement('div');
    xContainer.style.marginBottom = '10px';
    questionSection.appendChild(xContainer);
    createInputWithLabel(xContainer, `inputX${index}`, 'X:', 'number', {
      required: 'true',
      min: '0',
      step: '0.01'
    });

    const yContainer = document.createElement('div');
    yContainer.style.marginBottom = '10px';
    questionSection.appendChild(yContainer);
    createInputWithLabel(yContainer, `inputY${index}`, 'Y:', 'number', {
      required: 'true',
      min: '0',
      step: '0.01'
    });
  });
}

function SBHandle(data) {

  const questionsData = JSON.parse(data.value);
  const typeSInputs = document.getElementById('typeSInputs');

  questionsData.forEach((question, index) => {

    // Create a section for each question with a unique ID
    const questionSection = createElement('div', { id: `question${index}`, style: 'margin-bottom: 30px; border: 1px solid #ccc; padding: 20px;' }, typeSInputs);

    if (question.type === 'count') {
      // Add the question text
      createElement('p', { textContent: question.questionText, style: 'margin-bottom: 20px;' }, questionSection);

      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionSection);
      createInputWithLabel(tempContainer, 'countInput', 'Count:', 'text', {
        required: 'true',
        pattern: '-?[0-9]*(\\.[0-9]+)?',
        title: 'Enter a valid number.'
      });

    }
    else if (question.type === 'locate') {

      // Add the question text
      createElement('p', { textContent: question.questionText, style: 'margin-bottom: 20px;' }, questionSection);

      // Inputs for X and Y coordinates
      const xContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionSection);
      createInputWithLabel(xContainer, `inputX${index}`, 'X:', 'number', {
        required: 'true',
        min: '0',
        step: '0.01'
      });

      const yContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionSection);
      createInputWithLabel(yContainer, `inputY${index}`, 'Y:', 'number', {
        required: 'true',
        min: '0',
        step: '0.01'
      });
    }
    else if (question.type === 'locate') {

      // Add the question text
      createElement('p', { textContent: question.questionText, style: 'margin-bottom: 20px;' }, questionSection);

      // Inputs for X and Y coordinates
      const xContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionSection);
      createInputWithLabel(xContainer, `inputX${index}`, 'X:', 'number', {
        required: 'true',
        min: '0',
        step: '0.01'
      });

      const yContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionSection);
      createInputWithLabel(yContainer, `inputY${index}`, 'Y:', 'number', {
        required: 'true',
        min: '0',
        step: '0.01'
      });
    }
    else if (question.type === 'text') {
      // Add the question text
      createElement('p', { textContent: question.questionText, style: 'margin-bottom: 20px;' }, questionSection);

      const tempContainer = createElement('div', { style: 'margin-bottom: 10px;' }, questionSection);
      createInputWithLabel(tempContainer, 'objectName', 'Object Name:', 'text', {
        required: 'true',
        title: 'Name.'
      });

    }

  });
}

function IAHandle(data) {
  const questions = JSON.parse(data.value);
  const container = document.getElementById('typeIInputs'); // Reference to the container

  questions.forEach((question, index) => {
    // Add margin to each input wrapper
    const inputWrapper = createElement('div', { style: 'margin-bottom: 20px;' }, container);
    // Create input with label, adding specific styles for margin between label and input
    createInputWithLabel(inputWrapper, `textBox${index + 1}`, `Question ${index + 1}: ${question}`, 'text', {
      required: 'true'
    }, {
      labelStyle: 'display: block; margin-bottom: 5px;',
      inputStyle: 'width: 100%;'
    });
  });
}


function IBHandle(data) {
  const questions = JSON.parse(data.value);
  const container = document.getElementById('typeIInputs'); // Reference to the container

  questions.forEach((item, index) => {
    switch (item.type) {
      case "info":
        createElement('div', { style: 'margin-bottom: 20px;', textContent: item.content }, container);
        break;
      case "question":
        const inputWrapper = createElement('div', { style: 'margin-bottom: 20px;' }, container);
        createInputWithLabel(inputWrapper, `textBox${index + 1}`, item.content, 'text', {
          required: 'true'
        }, {
          labelStyle: 'display: block; margin-bottom: 5px;',
          inputStyle: 'width: 100%;'
        });
        break;
      default:
        console.error(`Unknown type: ${item.type}`);
    }
  });
}
