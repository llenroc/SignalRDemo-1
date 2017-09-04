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
            function randomGrouping($group1, $group2, groupedList) {
                var median = groupedList.length / 2, html = "";
                for (var i = 0; i < groupedList.length; i++) {
                    html = '<li>' + groupedList[i].UserName + '</li>';
                    i < median ? group1.append(html) : group2.append(html);
                }
            }
            function mmrGrouping($group1, $group2, groupedList) {
                console.log(groupedList)
                var group1MMR = group2MMR = 0, group1Members = group2Members = 0, median = groupedList.length / 2, html = "";
                for (var i = 0; i < groupedList.length; i++) {
                    html = '<li>' + groupedList[i].UserName + '</li>';
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
            // Set initial focus to message input box.
            $('#message').focus();
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
                        $('.battle-vs').css({ opacity: 1 });
                    })
                    .delegate('#algorithm-select', 'click', function (e) {
                        $(e.target).siblings('span').text($('#algorithm-select option:selected').text());
                    })
            });
        },
        init: function () {
            var _this = this, userInfo = $.getCookie("userData");
            if (userInfo == "" || userInfo == null || typeof _this.roomId == "undefined" || _this.roomId == "") {
                alert("无效的用户信息");
                window.location.href = "/Home/RoomList";
            } else {
                _this.UserInfo = JSON.parse(userInfo);
            }
            _this.displayRoomData();
            _this.initSignalR();
            _this.bindEvents();
            
        }
    }
    page.init();
   
})