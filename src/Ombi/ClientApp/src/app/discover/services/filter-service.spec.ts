import { describe, it, expect, beforeEach, vi } from 'vitest';
import { FilterService } from './filter-service';
import { SearchFilter } from '../../my-nav/SearchFilter';

describe('FilterService', () => {
  let service: FilterService;

  beforeEach(() => {
    service = new FilterService();
  });

  it('should have undefined filter initially', () => {
    expect(service.filter).toBeUndefined();
  });

  it('should store the filter when changeFilter is called', () => {
    const filter = new SearchFilter({ movies: true, tvShows: false, music: false, people: false });
    service.changeFilter(filter);
    expect(service.filter).toEqual(filter);
  });

  it('should emit onFilterChange when changeFilter is called', () => {
    const spy = vi.fn();
    service.onFilterChange.subscribe(spy);

    const filter = new SearchFilter({ movies: true, tvShows: true, music: false, people: false });
    service.changeFilter(filter);

    expect(spy).toHaveBeenCalledWith(filter);
  });

  it('should emit the updated filter on subsequent changes', () => {
    const emissions: SearchFilter[] = [];
    service.onFilterChange.subscribe(f => emissions.push(f));

    const filter1 = new SearchFilter({ movies: true, tvShows: false, music: false, people: false });
    const filter2 = new SearchFilter({ movies: false, tvShows: true, music: true, people: false });

    service.changeFilter(filter1);
    service.changeFilter(filter2);

    expect(emissions).toHaveLength(2);
    expect(emissions[0]).toEqual(filter1);
    expect(emissions[1]).toEqual(filter2);
  });
});
