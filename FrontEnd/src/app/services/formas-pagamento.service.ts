import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FormaPagamentoDto } from '../models/models';
import { environment } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class FormasPagamentoService {
  private apiUrl = `${environment.apiUrl}/FormasPagamento`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<FormaPagamentoDto[]> {
    return this.http.get<FormaPagamentoDto[]>(this.apiUrl);
  }

  getAtivos(): Observable<FormaPagamentoDto[]> {
    return this.http.get<FormaPagamentoDto[]>(`${this.apiUrl}/ativos`);
  }

  getById(id: number): Observable<FormaPagamentoDto> {
    return this.http.get<FormaPagamentoDto>(`${this.apiUrl}/${id}`);
  }
}
