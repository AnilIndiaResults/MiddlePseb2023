jQuery(document).ready(function() {
   
   "use strict";
   
   // Tooltip
   jQuery('.tooltips').tooltip({ container: 'body'});
   
   // Popover
   jQuery('.popovers').popover();
   
   // Show panel buttons when hovering panel heading
   jQuery('.panel-heading').hover(function() {
      jQuery(this).find('.panel-btns').fadeIn('fast');
   }, function() {
      jQuery(this).find('.panel-btns').fadeOut('fast');
   });
   
   // Close Panel
   jQuery('.panel .panel-close').click(function() {
      jQuery(this).closest('.panel').fadeOut(200);
      return false;
   });
   
   // Minimize Panel
   jQuery('.panel .panel-minimize').click(function(){
      var t = jQuery(this);
      var p = t.closest('.panel');
      if(!jQuery(this).hasClass('maximize')) {
         p.find('.panel-body, .panel-footer').slideUp(200);
         t.addClass('maximize');
         t.find('i').removeClass('fa-minus').addClass('fa-plus');
         jQuery(this).attr('data-original-title','Maximize Panel').tooltip();
      } else {
         p.find('.panel-body, .panel-footer').slideDown(200);
         t.removeClass('maximize');
         t.find('i').removeClass('fa-plus').addClass('fa-minus');
         jQuery(this).attr('data-original-title','Minimize Panel').tooltip();
      }
      return false;
   });
   
   jQuery('.leftpanel .nav .parent > a').click(function() {
      
      var coll = jQuery(this).parents('.collapsed').length;
      
      if (!coll) {
         jQuery('.leftpanel .nav .parent-focus').each(function() {
            jQuery(this).find('.children').slideUp('fast');
            jQuery(this).removeClass('parent-focus');
         });
         
         var child = jQuery(this).parent().find('.children');
         if(!child.is(':visible')) {
            child.slideDown('fast');
            if(!child.parent().hasClass('active'))
               child.parent().addClass('parent-focus');
         } else {
            child.slideUp('fast');
            child.parent().removeClass('parent-focus');
         }
      }
      return false;
   });
   
   
   // Menu Toggle
   jQuery('.menu-collapse').click(function() {
      if (!$('body').hasClass('hidden-left')) {
         if ($('.headerwrapper').hasClass('collapsed')) {
            $('.headerwrapper, .mainwrapper').removeClass('collapsed');
         } else {
            $('.headerwrapper, .mainwrapper').addClass('collapsed');
            $('.children').hide(); // hide sub-menu if leave open
         }
      } else {
         if (!$('body').hasClass('show-left')) {
            $('body').addClass('show-left');
         } else {
            $('body').removeClass('show-left');
         }
      }
      return false;
   });
   
   // Add class nav-hover to mene. Useful for viewing sub-menu
   jQuery('.leftpanel .nav li').hover(function(){
      $(this).addClass('nav-hover');
   }, function(){
      $(this).removeClass('nav-hover');
   });
   
   // For Media Queries
   jQuery(window).resize(function() {
      hideMenu();
   });
   
   hideMenu(); // for loading/refreshing the page
   function hideMenu() {
      
      if($('.header-right').css('position') == 'relative') {
         $('body').addClass('hidden-left');
         $('.headerwrapper, .mainwrapper').removeClass('collapsed');
      } else {
         $('body').removeClass('hidden-left');
      }
      
      // Seach form move to left
      if ($(window).width() <= 360) {
         if ($('.leftpanel .form-search').length == 0) {
            $('.form-search').insertAfter($('.profile-left'));
         }
      } else {
         if ($('.header-right .form-search').length == 0) {
            $('.form-search').insertBefore($('.btn-group-notification'));
         }
      }
   }
   
   collapsedMenu(); // for loading/refreshing the page
   function collapsedMenu() {
      
      if($('.logo').css('position') == 'relative') {
         $('.headerwrapper, .mainwrapper').addClass('collapsed');
      } else {
         $('.headerwrapper, .mainwrapper').removeClass('collapsed');
      }
   }
   
   /* For serach box in data table to search on press of ENTER */
   $("#txtSearch").on("keydown", function(event) {
	   if (event.keyCode == 13) { 
	   		if($('#btnSearch')) {
				$('#btnSearch').trigger('click'); 
			}
			return false; 
	   }
	});
	
	/* For serach box in data table to search on press of ENTER */
    $("#txtSearchDeleted").on("keydown", function(event) {
	   if (event.keyCode == 13) { 
	   		if($('#btnSearchDeleted')) {
				$('#btnSearchDeleted').trigger('click'); 
			}
			return false; 
	   }
	});
	
	/* For serach box in data table to search on press of ENTER */
    $("#txtSearchCD").on("keydown", function(event) {
	   if (event.keyCode == 13) { 
	   		if($('#btnSearchCD')) {
				$('#btnSearchCD').trigger('click'); 
			}
			return false; 
	   }
	});
	
	/* For serach box in data table to search on press of ENTER */
    $("#txtSearchCL").on("keydown", function(event) {
	   if (event.keyCode == 13) { 
	   		if($('#btnSearchCL')) {
				$('#btnSearchCL').trigger('click'); 
			}
			return false; 
	   }
	});
	

});

function showSiteMessage(title, message, msgtype) {
	if(msgtype == "") {
		msgtype = "primary";	
	}
	msgtype = 'growl-' + msgtype;
	$.gritter.add({
		title: title,
		text: message,
		class_name: msgtype,
		image: 'images/screen.png',
		sticky: false,
		time: ''
	});	
}

//Javascript

var monthNames = ["January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December"
];

function parseToDate(data) {
	var retVal = "";
	if(data != null) {
		var date = new Date(parseInt(data.replace("/Date(", "").replace(")/", "").trim()));
		var month = date.getMonth() + 1;
		//return (parseInt(month) > 9 ? month : "0" + month) + "/" + date.getDate() + "/" + date.getFullYear();
		retVal = date.getDate() + " " + monthNames[date.getMonth()] + ", " + date.getFullYear();
	}
    return retVal;
}

function parseToNumber(data) {
	var retVal = "";
	if(data != null) {		
		retVal = parseFloat(data).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
	}
    return retVal;
}

function parseToCurrency(data) {
	var retVal = "";
	if(data != null) {		
		retVal = '$' + parseFloat(data).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");
	}
    return retVal;
}

function convertNETDateTime(sNetDate) {
	if (sNetDate == null) return null;
	if (sNetDate instanceof Date) return sNetDate;
		var r = /\/Date\(([0-9]+)\)\//i
		var matches = sNetDate.match(r);
		if (matches.length == 2) {
			return new Date(parseInt(matches[1]));
		}
	else {
		return sNetDate;
	}
}

function policyStatusColorCode(data) {
	if(data == 'Prospect') { return '<span style="color:#0510e3;">' + data + '</span>'; }
	else if(data == 'Policy Received') { return '<span style="color:#00933f;">' + data + '</span>'; }
	else if(data == 'Cancelled') { return '<span style="color:#ff0000;">' + data + '</span>'; }
	else { return '<span style="color:#636E7B;">' + data + '</span>'; }
}


function parseToDateTime(data) {
    var retVal = "";
	if(data != null) {
		var date = new Date(parseInt(data.replace("/Date(", "").replace(")/", "").trim()));
		var month = date.getMonth() + 1;
		var hrs = (parseInt(date.getHours()) > 9 ? date.getHours() : "0" + date.getHours());
		var ampm = "am";
		if (hrs > 12) {
			hrs = hrs - 12;
			ampm = "pm";
		}
	
		//return (parseInt(month) > 9 ? month : "0" + month) + "/" + date.getDate() + "/" + date.getFullYear() + " " + (parseInt(date.getHours()) > 9 ? date.getHours() : "0" + date.getHours()) + ":" + (parseInt(date.getMinutes()) > 9 ? date.getMinutes() : "0" + date.getMinutes()) + ":" + (parseInt(date.getSeconds()) > 9 ? date.getSeconds() : "0" + date.getSeconds());
		retVal = date.getDate() + " " + monthNames[date.getMonth()] + ", " + date.getFullYear() + " " + hrs + ":" + (parseInt(date.getMinutes()) > 9 ? date.getMinutes() : "0" + date.getMinutes()) + " " + ampm
	}
    return retVal;
}

function parseBitRightOnlyValue(data) {
    retVal = '';
	if(data) {
		retVal = '<i class="fa fa-check-square-o" style="color:#2f9014; font-size:16px;" title="TRUE"></i>';
	}
    return retVal;
}

function parseBitValue(data) {
    retVal = '<i class="fa fa-times" style="color:#ff0000; font-size:16px;" title="FALSE"></i>';
	if(data) {
		retVal = '<i class="fa fa-check-square-o" style="color:#2f9014; font-size:16px;" title="TRUE"></i>';
	}
    return retVal;
}

function checkFileTypeSize(file, typesInComma, sizeInMb) {
    if (file.value != "") {
        var sFileName = file.value;
        var sFileExtension = sFileName.split('.')[sFileName.split('.').length - 1].toLowerCase();
        var iFileSize = file.files[0].size;
        var iConvert = (file.files[0].size / 1048576).toFixed(2);
        //extension checking
        var _typesInComma = "," + typesInComma + ",";
        if (_typesInComma.indexOf("," + sFileExtension + ",") == -1) {
            alert("Extension of the Uploaded file should match " + typesInComma);
            return false;
        }

        //size checking
        if (parseFloat(iConvert) > parseFloat(sizeInMb)) {
            alert("Size of the Uploaded file should not be greater than " + sizeInMb + " MB.");
            return false;
        }

    }
    return true;
}