import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AssuntoDto, CreateAssuntoDto, UpdateAssuntoDto } from '../models/models';
import { environment } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class AssuntosService {
  private apiUrl = `${environment.apiUrl}/Assuntos`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AssuntoDto[]> {
    return this.http.get<AssuntoDto[]>(this.apiUrl);
  }

  getAtivos(): Observable<AssuntoDto[]> {
    return this.http.get<AssuntoDto[]>(`${this.apiUrl}/ativos`);
  }

  getById(id: number): Observable<AssuntoDto> {
    return this.http.get<AssuntoDto>(`${this.apiUrl}/${id}`);
  }

  create(assunto: CreateAssuntoDto): Observable<number> {
    return this.http.post<number>(this.apiUrl, assunto);
  }

  update(id: number, assunto: UpdateAssuntoDto): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}`, assunto);
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
}
