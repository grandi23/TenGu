var monthNames = [
    "һ��", "����", "����", "����", "����", "����",
    "����", "����", "����", "ʮ��", "ʮһ��", "ʮ����"
];
var dayNames = [
    "������, ", "����һ, ", "���ڶ�, ", "������, ",
    "������, ", "������, ", "������, "
];

var newDate = new Date();
newDate.setDate(newDate.getDate() + 1);
$('#Date').html(newDate.getFullYear() + " " + dayNames[newDate.getDay()] + " " + newDate.getDate() + ' ' + monthNames[newDate.getMonth()]);
