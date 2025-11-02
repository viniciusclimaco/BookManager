import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LivroDto, CreateLivroDto, UpdateLivroDto } from '../models/models';
import { environment } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class LivrosService {
  private apiUrl = `${environment.apiUrl}/Livros`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<LivroDto[]> {
    return this.http.get<LivroDto[]>(this.apiUrl);
  }

  getAtivos(): Observable<LivroDto[]> {
    return this.http.get<LivroDto[]>(`${this.apiUrl}/ativos`);
  }

  getById(id: number): Observable<LivroDto> {
    return this.http.get<LivroDto>(`${this.apiUrl}/${id}`);
  }

  create(livro: CreateLivroDto): Observable<number> {
    return this.http.post<number>(this.apiUrl, livro);
  }

  update(id: number, livro: UpdateLivroDto): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}`, livro);
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
}
