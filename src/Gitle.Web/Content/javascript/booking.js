$(function () {
  var activeRow = undefined;
  $('.booking-edit').on('click', function (e) {
    e.preventDefault();
    var row = $(this).parents('.booking-row');
    if (activeRow != undefined) {
      activeRow.find('.booking-row-edit').remove();
      activeRow.find('.booking-row-content').show();
    }
    activeRow = row;
    $.ajax({
      method: 'GET',
      data: {
        id: row.data('id')
      },
      url: '/booking/edit',
      success: function (data) {
        row.find('.booking-row-content').hide();
        row.prepend(data);
        addEditListeners(row);
        bookingRowInit(row);

      }
    });
  });

  var addEditListeners = function (row) {
    row.find('.booking-edit-cancel').on('click', function (e) {
      e.preventDefault();
      row.find('.booking-row-edit').remove();
      row.find('.booking-row-content').show();
      activeRow = undefined;
    });
  };

  var datepickerOptions = {
    format: 'dd-mm-yyyy',
    weekStart: 1
  };

  var bookingRowInit = function (row) {

    row.find('.project-chooser').data('suggestion', undefined);

    row.find('.project-chooser').autocomplete({
      serviceUrl: '/project/autocomplete',
      autoSelectFirst: true,
      noCache: true,
      minChars: 0,
      onSelect: function (suggestion) {
        var projectChooser = row.find('.project-chooser');
        if (projectChooser.data('suggestion') != undefined && projectChooser.data('suggestion') == suggestion.data) return false;
        projectChooser.data('suggestion', suggestion.data).val(suggestion.value);
        row.find('.booking_Project_Id').val(suggestion.data);
        row.find('.issue-chooser').val('').autocomplete('setOptions', { params: { projectId: suggestion.data } });
        row.find('.booking_Issue_Id').val('');
      }
    }).on('focus', function () {
      $(this).autocomplete().onValueChange();
    });;

    row.find('.issue-chooser').autocomplete({
      serviceUrl: '/issue/autocomplete',
      params: { projectId: row.find('.booking_Project_Id').val() },
      autoSelectFirst: true,
      noCache: true,
      minChars: 0,
      width: row.find('.project-chooser').width(),
      onSelect: function (suggestion) {
        row.find('.booking_Issue_Id').val(suggestion.data);
        row.find('.issue-chooser').val(suggestion.value);
      }
    });

    // naar app.js, universeel maken door vanuit booking-parser te verwijzen naar output veld.
    var bookingInput = row.find('.booking_Minutes');
    $('.booking-parser').change(function (e) {
      var value = $(this).val().replace(',', '.');

      var timeRegex = /^([0-9]+):([0-9]{2})$/i;
      var decimalRegex = /^([0-9]+)\.([0-9]+)$/i;
      var numberRegex = /^[0-9]+$/i;

      var visualOutput = value;
      var hoursOutput = value;

      if (numberRegex.test(value)) {
        var number = parseInt(value);
        if (number > 9) {
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

    row.find('.date').fdatepicker(datepickerOptions);

  };

  bookingRowInit($('.booking-row-new'));

  $('.date').fdatepicker(datepickerOptions).on('show', function () {
    $('[data-dayshift]').removeClass('active');
  });

  $('[data-dayshift]').each(function () {
    var shift = $(this).data('dayshift');
    var label = '';
    switch (shift) {
      case 0:
        label = 'Vandaag';
        break;
      case -1:
        label = 'Gisteren';
        break;
      default:
        label = Date.today().add(shift).days().toString('ddd dd MMM');
    }
    $(this).text(label);
  }).click(function (e) {
    e.preventDefault();
    var today = Date.today();
    var shift = $(this).data('dayshift');
    var date = today.add(shift).days();
    $('#booking_Date').val(date.toString('dd-MM-yyyy'));
    $('[data-dayshift]').removeClass('active');
    $(this).addClass('active');
  }).filter('[data-dayshift=0]').click();



});