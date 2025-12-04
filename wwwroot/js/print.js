window.openPrintWindow = function (htmlContent) {
    const width = 900;
    const height = 700;
    const left = (window.screen.width - width) / 2;
    const top = (window.screen.height - height) / 2;

    let myWindow = window.open("", "_blank", `width=${width},height=${height},top=${top},left=${left},scrollbars=no,resizable=no`);
    myWindow.document.write(htmlContent);
    myWindow.document.close();
    myWindow.focus();
    myWindow.print();
    myWindow.close();
}
