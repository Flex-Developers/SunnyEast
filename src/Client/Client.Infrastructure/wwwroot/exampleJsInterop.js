// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

// Глобальная функция для сохранения байтов как файла.
// Этот файл подключается обычным <script> (НЕ module), поэтому НИКАКИХ 'export' здесь быть не должно.
window.downloadFileFromBytes = (fileName, contentType, bytes) => {
  try {
    const arr  = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);
    const blob = new Blob([arr], { type: contentType });
    const url  = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = fileName;

    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  } catch (e) {
    console.error('downloadFileFromBytes error:', e);
  }
};

