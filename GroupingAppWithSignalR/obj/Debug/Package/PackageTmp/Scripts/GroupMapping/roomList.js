$(function () {
    var page = {
        userInfoPopUp: $('#popupLogin'),
        header: $('#userNameContainer'),
        actionArea: $('.action-area'),
        roomList: $('.room-list'),
        UserInfo: {
            UserName: "",
            Stars: 1,
            UserLevel: '',
            RoomId: '',
            OpenId: '',
        },
        firstLogin: function () {
            return this.UserInfo.UserName == "";
        },
        buildLevel: function () {
            var _this = this;
            var levelArray = [["倔强青铜", 3], ["秩序白银", 3], ["荣耀黄金", 4], ["尊贵铂金", 5], ["永恒钻石", 5], ["至尊黑耀", 5], ["最强王者", 1]], html = "";
            for (var i = 0; i < levelArray.length; i++) {
                for (var j = levelArray[i][1]; j > 0; j--) {
                    var postFix = "";
                    switch (j) {
                        case 1:
                            postFix = "I"; break;
                        case 2:
                            postFix = "II"; break;
                        case 3:
                            postFix = "III"; break;
                        case 4:
                            postFix = "IV"; break;
                        case 5:
                            postFix = "V"; break;
                    }
                    var element = levelArray[i][0] == "最强王者" ? "最强王者" : levelArray[i][0] + postFix;
                    html += '<option value="' + element + '">' + element + '</option>';
                }
            }
            _this.userInfoPopUp.find('#selectLevel').html(html);
        },
        displayUserData: function (userName) {
            this.header.text("Welcome! " + this.UserInfo.UserName);
        },
        createRoom: function () {
            var _this = this;
            $.post('/Home/CreateRoom', _this.UserInfo).done(function (data) {
                if (data.Success) {
                    _this.UserInfo.RoomId = data.Entity.RoomId;
                    $.setCookie("userData", JSON.stringify(_this.UserInfo));
                    location.reload();
                    //console.log(_this.UserInfo)
                } else {
                    Location.href = '/Wechat/Login?state=' + location.pathname;
                }
            })
        },
        bindEvents: function () {
            var _this = this;
            _this.userInfoPopUp.delegate('.confirm-data', 'click', function (e) {
                e.preventDefault();
                var $form = $(e.currentTarget).parents('form');
                var stars = $form.find('#selectStar option:selected').val(), level = $form.find('#selectLevel option:selected').val();
                _this.UserInfo.Stars = +stars;
                _this.UserInfo.UserLevel = level;
                $.post('/Home/UpdateUserLevel', { user: _this.UserInfo })
                    .done(function (result) {
                        if (result.Success) {
                            $.setCookie("userData", JSON.stringify(_this.UserInfo));
                        } else {
                            alert("设置失败");
                        }
                    })
                _this.displayUserData();
                _this.userInfoPopUp.popup('close');
                return false;
            })
            _this.header.on('click', function (e) {
                e.preventDefault();
                _this.userInfoPopUp.popup("open")
                return false;
            })
            _this.actionArea.delegate('.create-room', 'click', function (e) {
                _this.createRoom();
            })
                .delegate('.refresh-room', 'click', function () {
                    location.reload();
                })
            _this.roomList.delegate('button.join-room', 'click', function (e) {
                e.preventDefault();
                var rId = $(e.target).parents('li').data('id');
                window.location.href = "/Home/GetRoom?roomId=" + rId + "&openId=" + _this.UserInfo.OpenId;
            })
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
                            desc: '就问你们来不来',
                            link: location.href,
                            imgUrl: _this.UserInfo.Avatar,
                            trigger: function (res) {
                                // 不要尝试在trigger中使用ajax异步请求修改本次分享的内容，因为客户端分享操作是一个同步操作，这时候使用ajax的回包会还没有返回
                                //alert('用户点击发送给朋友');
                            },
                            success: function (res) {
                               //  alert('已分享');
                            },
                            cancel: function (res) {
                               //  alert('已取消');
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
            _this.buildLevel();
            if (userInfo == "" || userInfo == null) {
                //alert("无效的用户信息")
                location.href = "/Wechat/Login?state=" + Math.round(Math.random() * 1000);
            } else {
                _this.UserInfo = JSON.parse(decodeURIComponent(userInfo));
                _this.wxSetup();
                _this.displayUserData();
            }
            _this.bindEvents();
        }
    }
    page.init();
})