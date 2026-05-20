import { describe, it, expect } from 'vitest';
import { OrderPipe } from './OrderPipe';

describe('OrderPipe', () => {
  const pipe = new OrderPipe();

  it('should sort objects by a property in ascending order', () => {
    const data = [
      { name: 'Charlie', age: 30 },
      { name: 'Alice', age: 25 },
      { name: 'Bob', age: 28 },
    ];
    const result = pipe.transform(data, 'name', 'asc');
    expect(result.map(d => d.name)).toEqual(['Alice', 'Bob', 'Charlie']);
  });

  it('should sort objects by a property in descending order', () => {
    const data = [
      { name: 'Charlie', age: 30 },
      { name: 'Alice', age: 25 },
      { name: 'Bob', age: 28 },
    ];
    const result = pipe.transform(data, 'age', 'desc');
    expect(result.map(d => d.age)).toEqual([30, 28, 25]);
  });

  it('should default to ascending order', () => {
    const data = [
      { value: 3 },
      { value: 1 },
      { value: 2 },
    ];
    const result = pipe.transform(data, 'value');
    expect(result.map(d => d.value)).toEqual([1, 2, 3]);
  });

  it('should handle empty array', () => {
    expect(pipe.transform([], 'name')).toEqual([]);
  });
});
