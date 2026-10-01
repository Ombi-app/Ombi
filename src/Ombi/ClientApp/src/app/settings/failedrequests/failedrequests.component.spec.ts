import { describe, it, expect, vi } from 'vitest';
import { FailedRequestsComponent } from './failedrequests.component';
import { of } from 'rxjs';

function createComponent(retryResult: any = { result: true }) {
  const mockRetryService = {
    getFailedRequests: vi.fn().mockReturnValue(of([
      { failedId: 1, requestId: 101, title: 'Movie A', type: 1, retryCount: 3, error: 'Timeout' },
      { failedId: 2, requestId: 202, title: 'Show B', type: 0, retryCount: 1, error: 'Not found' },
    ])),
    deleteFailedRequest: vi.fn().mockReturnValue(of(true)),
    retryFailedRequest: vi.fn().mockReturnValue(of(retryResult)),
  };
  const mockMessageService = {
    send: vi.fn(),
    sendRequestEngineResultError: vi.fn(),
  };
  const mockTranslateService = {
    instant: vi.fn((key: string) => key),
  };

  const comp = new FailedRequestsComponent(
    mockRetryService as any,
    mockMessageService as any,
    mockTranslateService as any
  );
  return { comp, mockRetryService, mockMessageService, mockTranslateService };
}

describe('FailedRequestsComponent', () => {
  it('should load failed requests on init', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.vm).toHaveLength(2);
    expect(comp.vm[0].title).toBe('Movie A');
  });

  it('loads the failure metadata used by every table column', () => {
    const { comp } = createComponent();
    comp.ngOnInit();

    expect(comp.vm[0].type).toBe(1);
    expect(comp.vm[0].retryCount).toBe(3);
    expect(comp.vm[0].error).toBe('Timeout');
    expect(comp.vm[1].type).toBe(0);
    expect(comp.vm[1].error).toBe('Not found');
  });

  it('should remove a failed request and update the list', () => {
    const { comp, mockRetryService } = createComponent();
    comp.ngOnInit();
    const toRemove = comp.vm[0];
    comp.remove(toRemove);
    expect(mockRetryService.deleteFailedRequest).toHaveBeenCalledWith(1);
    expect(comp.vm).toHaveLength(1);
    expect(comp.vm[0].title).toBe('Show B');
  });

  it('reprocesses the selected queue row and removes it after success', () => {
    const { comp, mockRetryService, mockMessageService } = createComponent({ result: true });
    comp.ngOnInit();
    const toRetry = comp.vm[1];

    comp.reprocess(toRetry);

    expect(mockRetryService.retryFailedRequest).toHaveBeenCalledWith(2);
    expect(comp.vm).toHaveLength(1);
    expect(comp.vm[0].title).toBe('Movie A');
    expect(mockMessageService.send).toHaveBeenCalledWith('Requests.SuccessfullyReprocessed');
  });

  it('keeps the failed row visible when reprocessing still fails', () => {
    const failure = { result: false, errorMessage: 'Still failed' };
    const { comp, mockRetryService, mockMessageService } = createComponent(failure);
    comp.ngOnInit();
    const toRetry = comp.vm[1];

    comp.reprocess(toRetry);

    expect(mockRetryService.retryFailedRequest).toHaveBeenCalledWith(2);
    expect(comp.vm).toHaveLength(2);
    expect(mockMessageService.sendRequestEngineResultError).toHaveBeenCalledWith(failure);
  });

  it('should have correct columns defined', () => {
    const { comp } = createComponent();
    expect(comp.columnsToDisplay).toEqual(['title', 'type', 'retryCount', 'errorDescription', 'reprocessBtn', 'deleteBtn']);
  });
});
