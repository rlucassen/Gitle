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