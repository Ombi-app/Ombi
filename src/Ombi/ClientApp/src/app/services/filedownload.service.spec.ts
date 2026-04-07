import { describe, it, expect, vi } from 'vitest';
import { FileDownloadService } from './filedownload.service';
import { of } from 'rxjs';

describe('FileDownloadService', () => {
  it('should call http.get with the URL and handle response', () => {
    const mockHttp = {
      get: vi.fn().mockReturnValue(of(new Blob(['test'], { type: 'text/plain' }))),
    };
    const service = Object.create(FileDownloadService.prototype);
    service.http = mockHttp;
    service.url = '/api/v2/system/';
    service.headers = {};

    // Mock window.open and URL.createObjectURL
    const mockOpen = vi.spyOn(window, 'open').mockImplementation(() => null);
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:test');

    service.downloadFile('/test/file', 'text/plain');
    expect(mockHttp.get).toHaveBeenCalledWith('/test/file');

    mockOpen.mockRestore();
  });
});
