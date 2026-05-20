import { describe, it, expect, vi, beforeEach } from 'vitest';
import { IssuesService } from './issues.service';
import { IssueStatus } from '../interfaces';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
    post: vi.fn().mockReturnValue(of(true)),
    delete: vi.fn().mockReturnValue(of(true)),
  };

  const service = Object.create(IssuesService.prototype);
  service.http = mockHttp;
  service.url = '/api/v1/Issues/';
  service.headers = { set: vi.fn() };

  return { service: service as IssuesService, mockHttp };
}

describe('IssuesService', () => {
  let service: IssuesService;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getCategories', () => {
    service.getCategories();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/categories/', expect.anything());
  });

  it('should call POST for createCategory', () => {
    const cat = { id: 0, value: 'Audio' } as any;
    service.createCategory(cat);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Issues/categories/', JSON.stringify(cat), expect.anything());
  });

  it('should call DELETE for deleteCategory', () => {
    service.deleteCategory(5);
    expect(mockHttp.delete).toHaveBeenCalledWith('/api/v1/Issues/categories/5', expect.anything());
  });

  it('should call GET for getIssues', () => {
    service.getIssues();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/', expect.anything());
  });

  it('should call GET for getIssuesByRequestId', async () => {
    await service.getIssuesByRequestId(10);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/request/10', expect.anything());
  });

  it('should call GET for getIssuesByProviderId', async () => {
    await service.getIssuesByProviderId('tmdb-123');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/provider/tmdb-123', expect.anything());
  });

  it('should call GET for getIssuesPage with correct params', () => {
    service.getIssuesPage(10, 0, IssueStatus.Pending);
    expect(mockHttp.get).toHaveBeenCalledWith(`/api/v1/Issues/10/0/${IssueStatus.Pending}`, expect.anything());
  });

  it('should call GET for getIssuesCount', () => {
    service.getIssuesCount();
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/count', expect.anything());
  });

  it('should call POST for createIssue', () => {
    const issue = { title: 'Broken audio' } as any;
    service.createIssue(issue);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Issues/', JSON.stringify(issue), expect.anything());
  });

  it('should call GET for getIssue by id', () => {
    service.getIssue(7);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/7', expect.anything());
  });

  it('should call GET for getComments', () => {
    service.getComments(7);
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v1/Issues/7/comments', expect.anything());
  });

  it('should call POST for addComment', () => {
    const comment = { issueId: 7, comment: 'Still broken' } as any;
    service.addComment(comment);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Issues/comments', JSON.stringify(comment), expect.anything());
  });

  it('should call POST for updateStatus', () => {
    const model = { issueId: 7, status: IssueStatus.Resolved } as any;
    service.updateStatus(model);
    expect(mockHttp.post).toHaveBeenCalledWith('/api/v1/Issues/status', JSON.stringify(model), expect.anything());
  });

  it('should call DELETE for deleteComment', () => {
    service.deleteComment(3);
    expect(mockHttp.delete).toHaveBeenCalledWith('/api/v1/Issues/comments/3', expect.anything());
  });

  it('should call DELETE for deleteIssue', async () => {
    await service.deleteIssue(7);
    expect(mockHttp.delete).toHaveBeenCalledWith('/api/v1/Issues/7', expect.anything());
  });
});
