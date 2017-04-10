/*
 * input element that needs to parse its value into something of a readable time, writes the number of minutes to [data-minutes-field]
 */
$.fn.bookingParser = function () {
  return this.change(function () {
    var bookingInput = $($(this).data('minutes-field'));
    var value = $(this).val().replace(',', '.');

    var timeRegex = /^([0-9]+):([0-9]{2})$/i;
    var decimalRegex = /^([0-9]+)\.([0-9]+)$/i;
    var numberRegex = /^[0-9]+$/i;

    var visualOutput = value;
    var hoursOutput = value;

    if (numberRegex.test(value)) {
      var number = parseInt(value);
      if (number > 9 && number <= 60) {
        var fullhours = parseInt(number / 60);
        var fullminutes = number - (fullhours * 60);
        visualOutput = fullhours + ':' + (fullminutes < 10 ? "0" + fullminutes : fullminutes);
        hoursOutput = number;
      } else {
        visualOutput = number + ':00';
        hoursOutput = number * 60;
      }
    }
    if (timeRegex.test(value)) {
      var parts = value.split(':');
      var hours = parseFloat(parts[0]);
      var minutes = parseFloat(parts[1]);
      hoursOutput = (hours * 60 + minutes);
    }
    if (decimalRegex.test(value)) {
      var hours = parseFloat(value);
      hoursOutput = (hours * 60);
      var fullhours = parseInt(value);
      var fullminutes = (hours - fullhours) * 60;
      visualOutput = fullhours + ':' + (fullminutes < 10 ? "0" + fullminutes : fullminutes);
    }

    $(this).val(visualOutput);
    bookingInput.val(hoursOutput);
  });
};

/*
 * Initializes the element with Gitle search functionality
 */
$.fn.gitleSearch = function () {
  return this.each(function () {
    var searchField = $(this);
    searchField.autocomplete({
      serviceUrl: '/search',
      autoSelectFirst: true,
      onSelect: function (suggestion) {
        window.location = suggestion.data;
      }
    });

    $(document).keydown(function (e) {
      if (e.which == 70 && e.ctrlKey && e.shiftKey) {
        searchField.focus();
      }
    });

  });
};

/*
 * Initializes the textarea as a gitle live comments textarea
 */
$.fn.liveComments = function () {
  return this.each(function () {
    var url = $(this).data('live-comments');
    var textarea = $(this);
    var staticComments = $('<div>').addClass('comments').html(marked(textarea.val()));
    var container = $('<div>').addClass('comments-container').insertAfter(textarea).append(staticComments).append(textarea);
    staticComments.click(function () {
      container.addClass('edit');
      textarea.focus();
    });
    textarea.blur(function () {
      $.ajax({
        url: url,
        method: 'POST',
        dataType: 'text',
        data: {
          comment: textarea.val()
        },
        success: function (data) {
          staticComments.html(marked(data));
          container.removeClass('edit');
        },
        error: function () {
          console.log('niet opgeslagen');
        }
      });
    });
  });
};

$.fn.startEndDatePreset = function () {
  return this.click(function () {
    $($(this).data('insert-startdate-to')).val($(this).data('insert-startdate'));
    $($(this).data('insert-enddate-to')).val($(this).data('insert-enddate'));
  });
}

$.fn.initProjectTypeNumbers = function () {
  return this.change(function () {
    var numberField = $($(this).data('numberfield'));

    var value = $(this).val();

    switch (value) {
      case "1":
        numberField.val(numberField.data('initial-number'));
        break;
      case "2":
      case "4":
        numberField.val(numberField.data('service-number'));
        break;
      case "3":
        numberField.val(numberField.data('internal-number'));
        break;
      default:
        numberField.val('');
    }

    $($(this).data('insert-startdate-to')).val($(this).data('insert-startdate'));
    $($(this).data('insert-enddate-to')).val($(this).data('insert-enddate'));
  });
}
