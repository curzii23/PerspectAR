function collectValuesFromContainer(containerId) {
  const container = document.getElementById(containerId);
  if (!container) {
      console.error('Container not found:', containerId);
      return [];
  }

  let values = [];
  const elements = container.querySelectorAll('input, textarea, select');
  elements.forEach(el => {
      if ((el.type === 'checkbox' || el.type === 'radio') && !el.checked) {
          return;
      }
      let value = el.value;
      if (value !== '') {
          values.push(`${el.id}: ${value}`);
      }
  });

  return values;
}


function createElement(type, attributes, parent) {
  const element = document.createElement(type);
  for (const key in attributes) {
    if (key === 'textContent') {
      element.textContent = attributes[key];
    } else {
      element.setAttribute(key, attributes[key]);
    }
  }
  if (parent) {
    parent.appendChild(element);
  }
  return element;
}


function createInputWithLabel(parent, id, labelText, inputType, inputAttributes, styles = {}) {
  const label = createElement('label', { for: id, textContent: labelText, style: styles.labelStyle || '' }, parent);
  const input = createElement('input', Object.assign({ type: inputType, id: id, name: id }, inputAttributes, { style: styles.inputStyle || '' }), parent);
  createElement('br', {}, parent);
  return input;
}


function createSelectWithLabel(parent, id, labelText, options) {
  createElement('label', { for: id, textContent: labelText }, parent);
  const select = createElement('select', { id: id, required: 'true' }, parent);
  options.forEach(option => {
    createElement('option', { value: option.value, textContent: option.text }, select);
  });
  createElement('br', {}, parent);
  return select;
}

function handleYearSelect(parent, yearRange) {
  const [startYear, endYear] = yearRange.split('-').map(Number);
  const yearOptions = [];
  for (let year = startYear; year <= endYear; year++) {
    yearOptions.push({ value: year, text: year.toString() });
  }
  createSelectWithLabel(parent, 'yearSelect', 'Year:', yearOptions);
}