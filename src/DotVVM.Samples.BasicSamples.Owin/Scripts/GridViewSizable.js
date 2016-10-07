var $jq = jQuery.noConflict();

$jq(function () {

    var onSampleResized = function (e) {
        var columns = $jq(e.currentTarget).find("th");
        var msg = "columns widths: ";
        columns.each(function () { msg += $jq(this).width() + "px; "; });
        $jq("#sizableGridTxt").html(msg);
    };

    $jq("#sizableGrid").colResizable({
        fixed: false,
        liveDrag: true,
        gripInnerHtml: "<div class='grip'></div>",
        draggingClass: "dragging",
        headerOnly: true,
        onResize: onSampleResized
    });

});