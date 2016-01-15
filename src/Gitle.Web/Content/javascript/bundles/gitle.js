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
$(function () {
  var computeHoursForIssue = function (issueNumber) {
    var issueLine = $('.invoiceline[data-issue=' + issueNumber + ']');
    var hoursButton = issueLine.find('.invoiceline-hours');
    var hours = parseFloat(hoursButton.text());
    var estimateButton = issueLine.find('.invoiceline-estimate');
    var estimate = parseFloat(estimateButton.text());
    if (hours < estimate) {
      estimateButton.addClass('success');
      hoursButton.addClass('alert');
    } else {
      estimateButton.addClass('alert');
      hoursButton.addClass('success');
    }
  };

  var computePriceForInvoiceLine = function(invoiceLine) {
    var hourPrice = parseFloat($('#invoice_HourPrice').val());
    var hours = parseFloat(invoiceLine.find('.invoiceline-hours-input').val().replace(',', '.'));
    var nill = parseInt(invoiceLine.find('.invoiceline-null').val());
    var price = hours * hourPrice * (1 - nill);
    invoiceLine.find('.invoiceline-price').val(price.toString().replace('.', ','));
    calculateTotals();
  };

  var calculateTotals = function() {
    var subtotalPrice = 0.0;
    $('.invoiceline').each(function() {
      var linePrice = parseFloat($(this).find('.invoiceline-price').val().replace(',', '.')) * (1 - parseInt($(this).find('.invoiceline-null').val()));
      subtotalPrice += linePrice;
    });
    $('#invoice_Subtotal').val(subtotalPrice.toString().replace('.', ','));
    var correctionTotalPrice = 0.0;
    $('.correctionline').each(function() {
      var correctionValue = $(this).find('.correctionline-price').val();
      if (correctionValue)
        correctionTotalPrice += parseFloat(correctionValue.replace(',', '.'));
    });
    var vat = parseInt($('.vatline .vatline-vat').val());
    var vatPrice = (subtotalPrice + correctionTotalPrice) * vat * 0.21;
    $('.vatline-price').val(vatPrice.toString().replace('.', ','));
    var totalPrice = subtotalPrice + correctionTotalPrice + vatPrice;
    $('#invoice_Total').val(totalPrice.toString().replace('.', ','));
  };

  $('.invoiceline-issue').each(function () {
    computeHoursForIssue($(this).data('issue'));
  });

  $('#invoice_HourPrice').keyup(function () {
    $('.invoiceline').each(function () {
      computePriceForInvoiceLine($(this));
    });
  }).keyup();

  calculateTotals();

  $('.invoiceline-hours-input').on('change keyup', function () {
    var invoiceLine = $(this).parents('.invoiceline');
    computePriceForInvoiceLine(invoiceLine);
    invoiceLine.find('.invoiceline-estimate, .invoiceline-hours').removeClass('active');
    var hours = parseFloat($(this).val().replace(',', '.'));
    var estimateHours = parseFloat(invoiceLine.find('.invoiceline-estimate').text().replace(',', '.'));
    var bookingHours = parseFloat(invoiceLine.find('.invoiceline-hours').text().replace(',', '.'));
    if (hours == estimateHours) {
      invoiceLine.find('.invoiceline-estimate').addClass('active');
    } else if (hours == bookingHours) {
      invoiceLine.find('.invoiceline-hours').addClass('active');
    }
  });

  $('.correctionline-price').keyup(function () {
    calculateTotals();
  });

  $('.invoiceline-estimate, .invoiceline-hours').click(function (e) {
    e.preventDefault();
    var estimate = $(this).text();
    var invoiceLine = $(this).parents('.invoiceline');
    invoiceLine.find('.invoiceline-hours-input').val(estimate).change();
    invoiceLine.find('.invoiceline-estimate, .invoiceline-hours').removeClass('active');
    $(this).addClass('active');
  });

  $('.remove-line').click(function (e) {
    e.preventDefault();
    var line = $(this).parents('.invoiceline');
    if (line.data('issue')) {
      $('.booking[data-issue=' + line.data('issue') + ']').remove();
    }
    line.remove();
    calculateTotals();
  });

  $('.invoiceline-null-toggle').click(function (e) {
    e.preventDefault();
    var line = $(this).parents('.invoiceline');
    if ($(this).hasClass('null')) {
      $(this).removeClass('null');
      line.find('.invoiceline-null').val(0);
      line.find('.invoiceline-price').removeClass('null');
    } else {
      $(this).addClass('null');
      line.find('.invoiceline-null').val(1);
      line.find('.invoiceline-price').addClass('null');
    }
    computePriceForInvoiceLine(line);
    calculateTotals();
  });

  $('.vatline-toggle').click(function (e) {
    e.preventDefault();
    var line = $(this).parents('.vatline');
    if ($(this).hasClass('vat')) {
      $(this).removeClass('vat');
      line.find('.vatline-vat').val(1);
      line.find('.vatline-price').removeClass('null');
    } else {
      $(this).addClass('vat');
      line.find('.vatline-vat').val(0);
      line.find('.vatline-price').addClass('null');
    }
    calculateTotals();
  });
});
function GitleIssues() { }

GitleIssues.prototype = {
  init: function() {
    this.initThreeStateChecker();
    this.initGroupActions();
    this.initQuickView();
    this.initTimeParser();
  },

  initThreeStateChecker: function() {
    $('.three-state-checker').each(function () {
      var checker = $(this);
      checker.append('<i class="fa fa-square-o none"></i><i class="fa fa-check-square-o all"></i><i class="fa fa-minus-square-o some"></i>');
      var selector = $(this).data('selector');
      var checkboxes = $(selector);
      checkboxes.click(function () {
        if (checkboxes.length == checkboxes.filter(':checked').length) {
          checker.attr('data-state', 'all');
        } else if (checkboxes.filter(':checked').length === 0) {
          checker.attr('data-state', 'none');
        } else {
          checker.attr('data-state', 'some');
        }
      });
    }).click(function () {
      var selector = $(this).data('selector');
      var state = $(this).attr('data-state');
      var checkboxes = $(selector);
      switch (state) {
        case "none":
          checkboxes.attr('checked', 'checked');
          $(this).attr('data-state', 'all');
          break;
        default:
          checkboxes.removeAttr('checked');
          $(this).attr('data-state', 'none');
          break;
      }
      checkboxes.change();
    });
  },

  initGroupActions: function() {
    $('#group-actions').hide();
    $('.issues input[type=checkbox][name=issue]').change(function () {
      var checkedBoxes = $('.issues input[type=checkbox][name=issue]:checked');
      if (checkedBoxes.length > 0) {
        $('#group-actions, #exportselection').show();
      } else {
        $('#group-actions, #exportselection').hide();
      }
      var values = [];
      checkedBoxes.each(function () {
        values.push($(this).val());
      });
      $('#group-actions [name=issues]').val(values.join(','));
    }).change();

    $('[data-group-action]').click(function (e) {
      e.preventDefault();
      var issues = $('#group-actions input[name=issues]').val();
      var href = $(this).prop('href');
      location.href = href + (href.indexOf('?') == -1 ? '?' : '&') + 'issues=' + issues;
    });

    $('.issue').click(function () {
      var checkbox = $(this).find('input[type=checkbox]');
      checkbox.prop('checked', !checkbox.is(':checked')).change();
    }).on('click', 'a, input', function (e) {
      e.stopPropagation();
    });
  },

  initQuickView: function() {
    var quickviewDelay = 800;
    $('[data-quickview]').hover(function () {
      var link = $(this);
      if ($(this).data('tooltip') == undefined) {
        var offset = $(this).position();
        var tooltip = $('<div class="quickview">').css('top', offset.top + $(this).height()).css('left', offset.left);
        $(this).data('tooltip', tooltip);
        tooltip.load($(this).data('quickview'), function () {
          tooltip.find('.marked').each(function () {
            $(this).html(marked($(this).html()));
          });
          link.append(tooltip.hide());
          link.data('timeout', setTimeout(function () {
            if (link.is(':hover')) {
              link.data('tooltip').show();
            }
          }, quickviewDelay));
        });
      } else {
        link.data('timeout', setTimeout(function () {
          if (link.is(':hover')) {
            link.data('tooltip').show();
          }
        }, quickviewDelay));
      }
    }, function () {
      $('.quickview').hide();
    });
  },

  initTimeParser: function() {
    $('.time-parser').blur(function () {
      var value = $(this).val();
      if (value === "" || value === "0") return;
      value = value.replace(/[A-Za-z$-]/g, "");
      value = value.replace(",", ".");
      value = parseFloat(value);
      var lessThanQuarter = value < 0.25;
      if (lessThanQuarter) {
        value = 0.25;
      }
      value = value.toString().replace(".", ",");
      $(this).val(value);
      if (lessThanQuarter) {
        $(this).error("minimum is een kwartier");
      }
    });
  }
};

var gitleIssues = null;
$(function () {
  gitleIssues = new GitleIssues();
  gitleIssues.init();
});