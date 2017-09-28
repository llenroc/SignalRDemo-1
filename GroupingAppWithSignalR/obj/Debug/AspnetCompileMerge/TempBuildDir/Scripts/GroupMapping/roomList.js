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
            UnionId: '',
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
                console.log(data)
                if (data.Success) {
                    _this.UserInfo.RoomId = data.Entity.RoomId;
                    $.setCookie("userData", JSON.stringify(_this.UserInfo));
                    location.reload();
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
                var userName = $form.find('input[name=user]').val(), level = $form.find('#selectLevel option:selected').val();
                if (userName == '' || level == '') {
                    alert("你不填用户名我怎么知道你是谁?");
                } else {
                    _this.UserInfo.UserName = userName;
                    _this.UserInfo.UserLevel = level;
                    $.setCookie("userData", JSON.stringify(_this.UserInfo));
                    _this.displayUserData();
                    _this.userInfoPopUp.popup('close');
                }
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
                window.location.href = "/Home/GetRoom?roomId=" + rId + "&unionId=" + _this.UserInfo.UnionId;
                //$.post('/Home/JoinRoom', { roomId: rId, user: _this.UserInfo }).done(function (data) {
                //    if (data.Status == 200 || data.Status == 303) {
                //        window.location.href = "/Home/GetRoom?roomId=" + rId;
                //    } else {
                //        alert(data.Message);
                //    }
                       
                //})
            })
        },
        init: function () {
            var _this = this, userInfo = $.getCookie("userData");
            _this.buildLevel();
            if (userInfo == "" || userInfo == null) {
                location.href = "/Wechat/Login?state=" + Math.round(Math.random() * 1000);
            } else {
                _this.UserInfo = JSON.parse(userInfo);
                _this.displayUserData();
                console.log(_this.UserInfo);
            }
            _this.bindEvents();
        }
    }
    page.init();
})