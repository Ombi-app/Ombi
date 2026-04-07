import { describe, it, expect, vi } from 'vitest';
import { AdvancedSearchDialogDataService } from './advanced-search-dialog-data.service';
import { RequestType } from '../../interfaces';

describe('AdvancedSearchDialogDataService', () => {
  let service: AdvancedSearchDialogDataService;

  beforeEach(() => {
    service = new AdvancedSearchDialogDataService();
  });

  describe('setData / getData', () => {
    it('should store and retrieve data', () => {
      const data = [{ id: 1, title: 'Movie' }];
      service.setData(data, RequestType.movie);
      expect(service.getData()).toBe(data);
    });

    it('should emit onDataChange when data is set', () => {
      const spy = vi.fn();
      service.onDataChange.subscribe(spy);
      service.setData({ test: true }, RequestType.tvShow);
      expect(spy).toHaveBeenCalledWith({ test: true });
    });
  });

  describe('getType', () => {
    it('should return the type set via setData', () => {
      service.setData([], RequestType.album);
      expect(service.getType()).toBe(RequestType.album);
    });
  });

  describe('setOptions / getOptions', () => {
    it('should store and retrieve options', () => {
      service.setOptions([1, 2], [3], [4], 2024, RequestType.movie, 0);
      const opts = service.getOptions();
      expect(opts.watchProviders).toEqual([1, 2]);
      expect(opts.genres).toEqual([3]);
      expect(opts.keywords).toEqual([4]);
      expect(opts.releaseYear).toBe(2024);
      expect(opts.type).toBe(RequestType.movie);
      expect(opts.position).toBe(0);
    });

    it('should emit onOptionsChange when options are set', () => {
      const spy = vi.fn();
      service.onOptionsChange.subscribe(spy);
      service.setOptions([], [], [], 2023, RequestType.tvShow, 5);
      expect(spy).toHaveBeenCalled();
    });
  });
});
