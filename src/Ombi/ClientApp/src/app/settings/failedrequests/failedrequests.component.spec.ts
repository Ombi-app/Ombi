import { describe, it, expect, vi } from 'vitest';
import { FailedRequestsComponent } from './failedrequests.component';
import { of } from 'rxjs';

function createComponent() {
  const mockRetryService = {
    getFailedRequests: vi.fn().mockReturnValue(of([
      { failedId: 1, title: 'Movie A', type: 1, retryCount: 3, errorDescription: 'Timeout' },
      { failedId: 2, title: 'Show B', type: 0, retryCount: 1, errorDescription: 'Not found' },
    ])),
    deleteFailedRequest: vi.fn().mockReturnValue(of(true)),
  };

  const comp = new FailedRequestsComponent(mockRetryService as any);
  return { comp, mockRetryService };
}

describe('FailedRequestsComponent', () => {
  it('should load failed requests on init', () => {
    const { comp } = createComponent();
    comp.ngOnInit();
    expect(comp.vm).toHaveLength(2);
    expect(comp.vm[0].title).toBe('Movie A');
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

  it('should have correct columns defined', () => {
    const { comp } = createComponent();
    expect(comp.columnsToDisplay).toEqual(['title', 'type', 'retryCount', 'errorDescription', 'deleteBtn']);
  });
});
