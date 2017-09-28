$(function () {
    var page = {
        roomId: $.getQueryStringByName("roomId"),
        header: $('#roomNameContainer'),
        chatContainer: $('#discussion'),
        actionArea: $('.action-area'),
        UserInfo: {
            UserName: "",
            Stars: 1,
            UserLevel: '',
            RoomId: '',
        },
        renderUserList: function (roomId) {
            var _this = this;
            $.get('/Home/RenderRoomUsers', { roomId: roomId })
                .done(function (data) {
                    $('.room-users ul').html(data);
                })
        },
        displayRoomData: function (userName) {
            this.header.text("房间号 " + this.roomId);
        },
        renderRLActions: function (isRoomLeader) {
            var _this = this;
            var setGroupBtn = '<div class="ui-grid-a"><div class="ui-block-a"><div class="ui-select"><div id="algorithm-select-button" class="ui-btn ui-icon-carat-d ui-btn-icon-right ui-corner-all"><span>随机</span><select id="algorithm-select" name="algorithm-select" data-shadow="false"><option value="1">随机</option><option value="2">段位</option></select></div></div></div><div class="ui-block-b"><div class="ui-btn ui-input-btn ui-corner-all ui-shadow">分组<input type="button" id="assigngroup" name="assigngroup" value="分组"></div></div></div>';
            if (isRoomLeader) {
                _this.actionArea.append(setGroupBtn)
            }
            
        },
        renderGroup: function (groupedList, algorithm) {
            var _this = this;
            var group1 = $('.group-1 ul'), group2 = $('.group-2 ul');
            group1.html(""); group2.html("");
            switch (+algorithm) {
                case 1:
                    randomGrouping(group1, group2, groupedList);
                    break;
                case 2:
                    mmrGrouping(group1, group2, groupedList);
                    break;
            }
            $('.battle-vs').css({ opacity: 1 });
            function randomGrouping($group1, $group2, groupedList) {
                var median = groupedList.length / 2, html = "";
                for (var i = 0; i < groupedList.length; i++) {
                    html = '<li><img src="' + groupedList[i].Avatar + '" width="35" /><br />"' + groupedList[i].UserName + '</li>';
                    i < median ? group1.append(html) : group2.append(html);
                }
            }
            function mmrGrouping($group1, $group2, groupedList) {
                var group1MMR = group2MMR = 0, group1Members = group2Members = 0, median = groupedList.length / 2, html = "";
                for (var i = 0; i < groupedList.length; i++) {
                    html = '<li><img src="' + groupedList[i].Avatar + '" width="35" /><br />"' + groupedList[i].UserName + '</li>';
                    if (group1MMR <= group2MMR) {
                        if (group1Members < median) {
                            group1MMR += +groupedList[i].MMR;
                            group1Members++;
                            $group1.append(html);
                        } else {
                            group2MMR += +groupedList[i].MMR;
                            group2Members++;
                            $group2.append(html);
                        }
                    } else {
                        if (group2Members < median) {
                            group2MMR += +groupedList[i].MMR;
                            group2Members++;
                            $group2.append(html);
                        } else {
                            group1MMR += +groupedList[i].MMR;
                            group1Members++;
                            $group1.append(html);
                        }
                    }
                }
            }
        },
        bindEvents: function () {
            var _this = this;
            
        },
        initSignalR: function () {
            var _this = this;
            var chat = $.connection.groupMappingHub;
            chat.client.addNewMessageToPage = function (name, message) {
                // Add the message to the page.
                $('#discussion').append('<li><strong>' + $.htmlEncode(name)
                    + '</strong>: ' + $.htmlEncode(message) + '</li>');
            };
            chat.client.renderUserList = function () {
                _this.renderUserList(_this.roomId)
            };
            chat.client.showErrorMessage = function (message) {
                alert(message);
                location.href = "/Home/RoomList";
            }
            chat.client.renderRLActions = function (isRoomLeader) {
                _this.renderRLActions(isRoomLeader);
            }
            chat.client.renderGroup = function (groupedList, algorithm) {
                _this.renderGroup(groupedList, algorithm);
            }
            // Get the user name and store it to prepend to messages.
            $('#displayname').val(_this.UserInfo.UserName);
            // Start the connection.
            $.connection.hub.start().done(function () {
                chat.server.joinRoom(_this.roomId, _this.UserInfo);
                $('#sendmessage').click(function () {
                    // Call the Send method on the hub.
                    if ($('#message').val() != "") {
                        var channel = $('#channel-select option:selected').val();
                        switch (+channel) {
                            case 2:
                                chat.server.sendToAll($('#displayname').val(), $('#message').val()); break;
                            case 1:
                            default:
                                chat.server.sendToGroup($('#displayname').val(), $('#message').val(), _this.roomId); break;
                        }
                        _this.chatContainer.animate({
                            scrollTop: _this.chatContainer.offset().top
                        }, 150)
                    }
                   
                    // Clear text box and reset focus for next comment.
                    $('#message').val('').focus();
                });
                _this.actionArea
                    .delegate('#assigngroup', 'click', function (e) {
                        var algorithm = $('#algorithm-select option:selected').val();
                        switch (+algorithm) {
                            case 1:
                                chat.server.randomGrouping(_this.roomId);
                                break;
                            case 2:
                                chat.server.mMRGrouping(_this.roomId);
                                break;
                            default:
                                chat.server.randomGrouping(_this.roomId);
                                break;
                        }
                        
                    })
                    .delegate('#algorithm-select', 'click', function (e) {
                        $(e.target).siblings('span').text($('#algorithm-select option:selected').text());
                    })
            });
        },
        wxSetup: function () {
            var _this = this;
            $.get('/Wechat/GetSignature', function (sign) {
                if (sign != '') {
                    var timestamp = new Date().getTime(), nonceStr = $.randomString(16);
                    var jsapi_ticket = "jsapi_ticket=" + sign + "&noncestr=" + nonceStr + "&timestamp=" + timestamp + "&url=" + location.href;
                    var encryptedTicket = hex_sha1(jsapi_ticket);
                    wx.config({
                        debug: false,
                        appId: 'wxe66445f68cbc9b8c', // 必填，公众号的唯一标识
                        timestamp: timestamp, // 必填，生成签名的时间戳
                        nonceStr: nonceStr, // 必填，生成签名的随机串
                        signature: encryptedTicket,// 必填，签名，见附录1
                        jsApiList: ['onMenuShareAppMessage'] // 必填，需要使用的JS接口列表，所有JS接口列表见附录2
                    });
                    wx.ready(function () {
                        wx.onMenuShareAppMessage({
                            title: '开黑9=1',
                            desc: '小黑屋已开好，就问你们来不来',
                            link: location.href,
                            imgUrl: _this.UserInfo.Avatar,
                            trigger: function (res) {
                                // 不要尝试在trigger中使用ajax异步请求修改本次分享的内容，因为客户端分享操作是一个同步操作，这时候使用ajax的回包会还没有返回
                                //alert('用户点击发送给朋友');
                            },
                            success: function (res) {
                               // alert('已分享');
                            },
                            cancel: function (res) {
                               // alert('已取消');
                            },
                            fail: function (res) {
                                alert(JSON.stringify(res));
                            }
                        });

                    })
                }
            })

        },
        init: function () {
            var _this = this, userInfo = $.getCookie("userData");
            if (userInfo == "" || userInfo == null || typeof _this.roomId == "undefined" || _this.roomId == "") {
                //alert("无效的用户信息");
                window.location.href = "/Home/RoomList";
            } else {
                _this.UserInfo = JSON.parse(decodeURIComponent(userInfo));
            }
            _this.wxSetup();
            _this.displayRoomData();
            _this.initSignalR();
            _this.bindEvents();
            
        }
    }
    page.init();
   
})