var toSql_Editor;
var fromSql_Editor;

window.SetFromSqlCode = (code) => {
    fromSql_Editor.getDoc().setValue(code);
}
window.GetFromSqlCode = () => {
    return fromSql_Editor.getDoc().getValue();
}
window.SetToSqlCode = (code) => {
    toSql_Editor.getDoc().setValue(code);
}
window.GetToSqlCode = () => {
    return toSql_Editor.getDoc().getValue();
}

window.CodeMirrorFromSql = (code) => {
    var mime = 'text/x-mssql';
    if (code = '') code = '\n\n\n\n\n\n\n\n\n\n';
    fromSql_Editor = CodeMirror.fromTextArea(document.getElementById('fromSql'), {
        mode: mime,
        indentWithTabs: true,
        smartIndent: true,
        lineNumbers: true,
        matchBrackets: true,
        autofocus: true,
        extraKeys: { "Ctrl-Space": "autocomplete" },
        viewportMargin: 100
    });
};

window.CodeMirrorToSql = (code) => {
    var mime = 'text/x-mssql';
    if (code = '') code = '\n\n\n\n\n\n\n\n\n\n';
    toSql_Editor = CodeMirror.fromTextArea(document.getElementById('toSql'), {
        mode: mime,
        indentWithTabs: true,
        smartIndent: true,
        lineNumbers: true,
        matchBrackets: true,
        autofocus: true,
        extraKeys: { "Ctrl-Space": "autocomplete" },
        viewportMargin: 100
    });
};