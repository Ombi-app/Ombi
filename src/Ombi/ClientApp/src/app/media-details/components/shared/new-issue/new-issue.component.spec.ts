import { describe, it, expect, vi } from 'vitest';
import { NewIssueComponent } from './new-issue.component';
import { RequestType, IssueStatus } from '../../../../interfaces';
import { of } from 'rxjs';

function createComponent() {
  const mockDialogRef = { close: vi.fn() };
  const data = { requestId: 42, requestType: RequestType.movie, title: 'Test Movie', providerId: 'tmdb-550', posterPath: '/poster.jpg' };
  const mockIssueService = {
    getCategories: vi.fn().mockReturnValue(of([{ id: 1, value: 'Audio' }, { id: 2, value: 'Video' }])),
    createIssue: vi.fn().mockReturnValue(of(1)),
  };
  const mockMessageService = { send: vi.fn() };
  const mockTranslate = { instant: vi.fn((key: string) => key) };

  const comp = new NewIssueComponent(mockDialogRef as any, data, mockIssueService as any, mockMessageService as any, mockTranslate as any);
  return { comp, mockDialogRef, mockIssueService, mockMessageService };
}

describe('NewIssueComponent', () => {
  it('should initialize issue with data from dialog', () => {
    const { comp } = createComponent();
    expect(comp.issue.requestId).toBe(42);
    expect(comp.issue.title).toBe('Test Movie');
    expect(comp.issue.status).toBe(IssueStatus.Pending);
  });

  it('should load categories on init', async () => {
    const { comp, mockIssueService } = createComponent();
    await comp.ngOnInit();
    expect(mockIssueService.getCategories).toHaveBeenCalled();
    expect(comp.issueCategories).toHaveLength(2);
  });

  it('should create issue and notify', async () => {
    const { comp, mockIssueService, mockMessageService } = createComponent();
    comp.issue.subject = 'No audio';
    comp.issue.description = 'Missing audio track';
    await comp.createIssue();
    expect(mockIssueService.createIssue).toHaveBeenCalled();
    expect(mockMessageService.send).toHaveBeenCalledWith('Issues.IssueDialog.IssueCreated');
  });

  it('should close dialog on onNoClick', () => {
    const { comp, mockDialogRef } = createComponent();
    comp.onNoClick();
    expect(mockDialogRef.close).toHaveBeenCalled();
  });
});
