import { describe, it, expect, vi, beforeEach } from 'vitest';
import { IssuesV2Service } from './issuesv2.service';
import { IssueStatus } from '../interfaces';
import { of } from 'rxjs';

function createService() {
  const mockHttp = {
    get: vi.fn().mockReturnValue(of([])),
  };

  const service = Object.create(IssuesV2Service.prototype);
  service.http = mockHttp;
  service.url = '/api/v2/Issues/';
  service.headers = { set: vi.fn() };

  return { service: service as IssuesV2Service, mockHttp };
}

describe('IssuesV2Service', () => {
  let service: IssuesV2Service;
  let mockHttp: ReturnType<typeof createService>['mockHttp'];

  beforeEach(() => {
    const mocks = createService();
    service = mocks.service;
    mockHttp = mocks.mockHttp;
  });

  it('should call GET for getIssues with position, take, and status', () => {
    service.getIssues(0, 10, IssueStatus.Pending);
    expect(mockHttp.get).toHaveBeenCalledWith(`/api/v2/Issues/0/10/${IssueStatus.Pending}`, expect.anything());
  });

  it('should call GET for getIssuesByProviderId', () => {
    service.getIssuesByProviderId('tmdb-456');
    expect(mockHttp.get).toHaveBeenCalledWith('/api/v2/Issues/details/tmdb-456', expect.anything());
  });
});
