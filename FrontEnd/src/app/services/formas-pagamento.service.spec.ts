import { TestBed } from '@angular/core/testing';

import { FormasPagamentoService } from './formas-pagamento.service';

describe('FormasPagamentoService', () => {
  let service: FormasPagamentoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FormasPagamentoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
