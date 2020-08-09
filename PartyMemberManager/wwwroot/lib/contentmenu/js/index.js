/*
===============================================================

Hi! Welcome to my little playground!

My name is Tobias Bogliolo. 'Open source' by default and always 'responsive',
I'm a publicist, visual designer and frontend developer based in Barcelona. 

Here you will find some of my personal experiments. Sometimes usefull,
sometimes simply for fun. You are free to use them for whatever you want 
but I would appreciate an attribution from my work. I hope you enjoy it.

===============================================================
*/
function ContextMenu() {
    $(".contextmenu").remove();
    this.contextMenu = $("<ul class='contextmenu'></ul>");
    this.addMenuItem=function(text, callback) {
        item = $("<li></li>")
        link = $("<a href='javascript:;'>" + text + "</a>");
        link.click(function () {
            $(".contextmenu").remove();
            callback();
        });
        item.append(link);
        this.contextMenu.append(item);
    }

    this.showMenu=function(event) {
        var left = event.clientX + Math.round($(document).scrollLeft(), 0);
        var top = event.clientY + Math.round($(document).scrollTop(), 0);
        $(document).find("body").append(this.contextMenu);
        showPopup(left, top,this.contextMenu);
        event.preventDefault();
        event.stopPropagation();
    }
}


$(document).ready(function () {
    //Hide contextmenu:
    $(document).mousedown(function (event) {
        var contextMenu = $(".contextmenu");
        if (contextMenu.length == 0) return;
        var left = event.clientX + Math.round($(document).scrollLeft(), 0);
        var top = event.clientY + Math.round($(document).scrollTop(), 0);
        if (left < contextMenu.offset().left || left > (contextMenu.offset().left + contextMenu.width()))
            $(".contextmenu").remove();
        else
            if (top < contextMenu.offset().top || top > (contextMenu.offset().top + contextMenu.height()))
                $(".contextmenu").remove();
    });
});
//Show contextmenu:
function showPopup(x, y, contextMenu) {
    //var contextMenu = $(".contextmenu");
    //Get window size:
    var winWidth = $(document).width();
    var winHeight = $(document).height();
    //Get pointer position:
    var posX = x;
    var posY = y;
    //Get contextmenu size:
    var menuWidth = contextMenu.width();
    var menuHeight = contextMenu.height();
    //Security margin:
    var secMargin = 10;
    //Prevent page overflow:
    if (posX + menuWidth + secMargin >= winWidth
        && posY + menuHeight + secMargin >= winHeight) {
        //Case 1: right-bottom overflow:
        posLeft = posX - menuWidth - secMargin + "px";
        posTop = posY - menuHeight - secMargin + "px";
    }
    else if (posX + menuWidth + secMargin >= winWidth) {
        //Case 2: right overflow:
        posLeft = posX - menuWidth - secMargin + "px";
        posTop = posY + secMargin + "px";
    }
    else if (posY + menuHeight + secMargin >= winHeight) {
        //Case 3: bottom overflow:
        posLeft = posX + secMargin + "px";
        posTop = posY - menuHeight - secMargin + "px";
    }
    else {
        //Case 4: default values:
        posLeft = posX + secMargin + "px";
        posTop = posY + secMargin + "px";
    };
    //Display contextmenu:
    contextMenu.css({
        "left": posLeft,
        "top": posTop
    }).show();
}
