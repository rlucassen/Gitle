/*
 * input element that needs to parse its value into something of a readable time, writes the number of minutes to [data-minutes-field]
 */
'use strict';

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
        var fullminutes = number - fullhours * 60;
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
      hoursOutput = hours * 60 + minutes;
    }
    if (decimalRegex.test(value)) {
      var hours = parseFloat(value);
      hoursOutput = hours * 60;
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
      onSelect: function onSelect(suggestion) {
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
        success: function success(data) {
          staticComments.html(marked(data));
          container.removeClass('edit');
        },
        error: function error() {
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
};

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
      success: function success(data) {
        row.find('.booking-row-content').hide();
        row.prepend(data);
        addEditListeners(row);
        bookingRowInit(row);
      }
    });
  });

  var addEditListeners = function addEditListeners(row) {
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

  var bookingRowInit = function bookingRowInit(row) {

    row.find('.project-chooser').data('suggestion', undefined);

    row.find('.project-chooser').autocomplete({
      serviceUrl: '/project/autocomplete',
      autoSelectFirst: true,
      noCache: true,
      minChars: 0,
      onSelect: function onSelect(suggestion) {
        var projectChooser = row.find('.project-chooser');
        if (projectChooser.data('suggestion') != undefined && projectChooser.data('suggestion') == suggestion.data) return false;
        projectChooser.data('suggestion', suggestion.data).val(suggestion.value);
        row.find('.booking_Project_Id').val(suggestion.data);
        row.find('.issue-chooser').val('').autocomplete('setOptions', { params: { projectId: suggestion.data } });
        row.find('.booking_Issue_Id').val('');
        if (suggestion.extraValue == "ticketRequired") {
          row.find('.booking_Issue_Id').prop("required", true);
          $(document).foundation('abide', 'reflow');
        } else {
          row.find('.booking_Comment').prop("required", true);
          $(document).foundation('abide', 'reflow');
        }
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
      onSelect: function onSelect(suggestion) {
        row.find('.booking_Issue_Id').val(suggestion.data);
        row.find('.issue-chooser').val(suggestion.value);
      }
    });

    row.find('.booking-row-null-toggle').click(function (e) {
      e.preventDefault();
      if ($(this).hasClass('null')) {
        $(this).removeClass('null');
        row.find('.booking_Unbillable').val(0);
      } else {
        $(this).addClass('null');
        row.find('.booking_Unbillable').val(1);
      }
    });

    row.find('.date').fdatepicker(datepickerOptions);
  };

  bookingRowInit($('.booking-row-new'));

  $('.date').fdatepicker(datepickerOptions).on('show', function () {
    $('[data-dayshift]').removeClass('active');
  });

  $('.reportdate').fdatepicker({
    format: 'yyyy-mm-dd',
    weekStart: 1
  }).on('changeDate, change', function (ev) {
    var query = $('#query').val();
    var target = $(ev.target);
    var prefix = target.data('filter-prefix');
    var rx = new RegExp(prefix + ":[a-zA-Z0-9-_,.]+");
    query = query.replace(rx, '');
    query = query + ' ' + prefix + ':' + target.val();
    $('#query').val(query.replace(/ +(?= )/g, ''));
    $('#query-form').submit();
  });

  var dayShiftFormat = 'dd MMM';
  if (new FoundationHelper().getCurrentSizeClass() === 'large') {
    dayShiftFormat = 'ddd dd MMM';
  }

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
        label = Date.today().add(shift).days().toString(dayShiftFormat);
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
  var computeHoursForIssue = function computeHoursForIssue(issueNumber) {
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

  var computePriceForInvoiceLine = function computePriceForInvoiceLine(invoiceLine) {
    var hourPrice = parseFloat($('#invoice_HourPrice').val());
    var hours = parseFloat(invoiceLine.find('.invoiceline-hours-input').val().replace(',', '.'));
    var nill = parseInt(invoiceLine.find('.invoiceline-null').val());
    var price = hours * hourPrice * (1 - nill);
    invoiceLine.find('.invoiceline-price').val(price.toFixed(2).toString().replace(".", ","));
    calculateTotals();
  };

  var calculateTotals = function calculateTotals() {
    var subtotalPrice = 0.0;
    $('.invoiceline').each(function () {
      var linePrice = parseFloat($(this).find('.invoiceline-price').val().replace(',', '.')) * (1 - parseInt($(this).find('.invoiceline-null').val()));
      subtotalPrice += linePrice;
    });
    $('#invoice_Subtotal').val(subtotalPrice.toFixed(2).toString().replace(".", ","));
    var correctionTotalPrice = 0.0;
    $('.correctionline').each(function () {
      var correctionValue = $(this).find('.correctionline-price').val();
      if (correctionValue) correctionTotalPrice += parseFloat(correctionValue.replace(',', '.'));
    });
    var vat = parseInt($('.vatline .vatline-vat').val());
    var vatPrice = (subtotalPrice + correctionTotalPrice) * vat * 0.21;
    $('.vatline-price').val(vatPrice.toFixed(2).toString().toString().replace(".", ","));
    var totalPrice = subtotalPrice + correctionTotalPrice + vatPrice;
    $('#invoice_Total').val(totalPrice.toFixed(2).toString().replace(".", ","));
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
function GitleIssues() {}

GitleIssues.prototype = {
  init: function init() {
    this.initFilters();
    this.initThreeStateChecker();
    this.initGroupActions();
    this.initQuickView();
    this.initTimeParser();
  },

  initFilters: function initFilters() {
    $('a[data-filter], a[data-filter-clear]').click(function (e) {
      e.preventDefault();
      var filter = $(this).data('filter');
      var oppositeFilter = $(this).data('filter-opposite');
      var query = $('#query').val();
      if ($(this).is('[data-filter-clear]')) query = '';
      if (query.indexOf(oppositeFilter) != -1) {
        query = query.replace(oppositeFilter, "");
      }
      if (oppositeFilter != undefined && oppositeFilter.indexOf(',') !== -1 && oppositeFilter.indexOf(':') === -1) {
        var opposites = oppositeFilter.split(',');
        for (var i in opposites) {
          var rx = new RegExp(opposites[i] + ":[a-zA-Z0-9-_,.]+");
          query = query.replace(rx, '');
        }
      }
      if (query.indexOf(filter) != -1) {
        query = query.replace(filter, "");
      } else {
        query = query + " " + filter;
      }
      $('#query').val(query.replace(/ +(?= )/g, ''));
      $('#query-form').submit();
    });
  },

  initThreeStateChecker: function initThreeStateChecker() {
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

  initGroupActions: function initGroupActions() {
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

  initQuickView: function initQuickView() {
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

  initTimeParser: function initTimeParser() {
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

