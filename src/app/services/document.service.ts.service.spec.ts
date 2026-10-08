import { TestBed } from '@angular/core/testing';

import { DocumentServiceTsService } from './document.service.ts.service';

describe('DocumentServiceTsService', () => {
  let service: DocumentServiceTsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocumentServiceTsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
