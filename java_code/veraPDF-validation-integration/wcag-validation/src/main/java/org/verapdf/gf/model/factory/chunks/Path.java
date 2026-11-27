/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.factory.chunks;

/**
 * Tracks the current point and subpath start point while PDF path construction operators are read.
 *
 * <p>Operators such as {@code m}, {@code l}, {@code c}, and {@code h} update these coordinates before a
 * later paint operator emits line-art chunks. Closing a path needs the saved start point, whereas each
 * subsequent segment begins at the current point.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested mutable value type or internal class:
 * {@code VeraPdf.Validation.Wcag.Model.Factory.Chunks.PathState}. The four coordinates map directly to
 * {@code double} fields or a pair of {@code Vector2} values; keep the two points distinct because closing
 * a path resets current coordinates to the subpath start.</p>
 *
 * @author Maxim Plushchov
 */
public class Path {

	private double currentX;
	private double currentY;
	private double startX;
	private double startY;

	public Path() {

	}

	public Path(double currentX, double currentY, double startX, double startY) {
		this.currentX = currentX;
		this.currentY = currentY;
		this.startX = startX;
		this.startY = startY;
	}

	public double getStartY() {
		return startY;
	}

	public void setStartY(double startY) {
		this.startY = startY;
	}

	public double getStartX() {
		return startX;
	}

	public void setStartX(double startX) {
		this.startX = startX;
	}

	public void setStartPoint(double startX, double startY) {
		setStartX(startX);
		setStartY(startY);
	}

	public double getCurrentX() {
		return currentX;
	}

	public void setCurrentX(double currentX) {
		this.currentX = currentX;
	}

	public double getCurrentY() {
		return currentY;
	}

	public void setCurrentY(double currentY) {
		this.currentY = currentY;
	}

	public void setCurrentPoint(double currentX, double currentY) {
		setCurrentX(currentX);
		setCurrentY(currentY);
	}

}
