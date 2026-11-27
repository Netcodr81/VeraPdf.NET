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

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSNumber;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.pd.PDExtGState;
import org.verapdf.pd.colors.PDColorSpace;
import org.verapdf.pd.font.PDFont;
import org.verapdf.wcag.algorithms.entities.content.LineChunk;

/**
 * Holds the graphics and text parameters that affect extraction at a point in a PDF content stream.
 *
 * <p>PDF operators save and restore graphics state with {@code q}/{@code Q}; this snapshot includes the
 * current transformation matrix, text state, fill color, line width, and line cap. {@link ChunkParser}
 * clones it when entering a Form XObject so local changes do not leak back to the caller. Color-space
 * operators can be disabled for Type 3 glyph programs, whose drawing describes glyph shapes rather than
 * the page's visible foreground color.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type: {@code VeraPdf.Validation.Wcag.Model.Factory.Chunks.GraphicsState}.
 * Use {@code Matrix3x2} only after confirming its multiplication convention matches PDF's six-value matrix;
 * otherwise retain a dedicated matrix type. Clone arrays and mutable state on copy, and represent nullable
 * font size as {@code double?}. The PDF resource/color model and WCAG {@code LineChunk} have no .NET port yet.</p>
 *
 * @author Maxim Plushchov
 */
public class GraphicsState implements Cloneable {

	private Matrix CTM = new Matrix();
	private TextState textState = new TextState();
	private double[] fillColor = new double[]{0};
	private PDColorSpace fillColorSpace;
	private boolean processColorOperators = true;
	private double lineWidth = 1.0;
	private int lineCap = LineChunk.BUTT_CAP_STYLE;

	private GraphicsState() {

	}

	public GraphicsState(PDResourcesHandler resourcesHandler) {
		this.fillColorSpace = resourcesHandler.getColorSpace(ASAtom.DEVICEGRAY);
	}

	public PDColorSpace getFillColorSpace() {
		return fillColorSpace;
	}

	public void setFillColorSpace(PDColorSpace fillColorSpace) {
		this.fillColorSpace = fillColorSpace;
	}

	public boolean isProcessColorOperators() {
		return processColorOperators;
	}

	public void disableColorOperators() {
		this.processColorOperators = false;
	}

	public Matrix getCTM() {
		return CTM;
	}

	public void setCTM(Matrix CTM) {
		this.CTM = CTM.clone();
	}

	public TextState getTextState() {
		return textState;
	}

	public void setTextState(TextState textState) {
		this.textState = textState.clone();
	}

	public double[] getFillColor() {
		return fillColor;
	}

	public void setFillColor(double[] fillColor) {
		this.fillColor = fillColor.clone();
	}

	public double getLineWidth() {
		return lineWidth;
	}

	public void setLineWidth(double lineWidth) {
		this.lineWidth = lineWidth;
	}

	public int getLineCap() {
		return lineCap;
	}

	public void setLineCap(int lineCap) {
		this.lineCap = lineCap;
	}

	public void copyProperties(GraphicsState graphicState) {
		// q/Q restores a complete snapshot; the setters that receive arrays or matrices clone their inputs.
		this.CTM = graphicState.getCTM();
		this.textState = graphicState.getTextState();
		this.fillColor = graphicState.getFillColor();
		this.fillColorSpace = graphicState.getFillColorSpace();
		this.processColorOperators = graphicState.isProcessColorOperators();
		this.lineWidth = graphicState.getLineWidth();
		this.lineCap = graphicState.getLineCap();
	}

	public void copyPropertiesFromExtGState(PDExtGState extGState) {
		if (extGState != null) {
			PDFont font = extGState.getFont();
			if (font != null) {
				this.getTextState().setTextFont(font);
			}
			COSNumber fontSize = extGState.getCOSFontSize();
			if (fontSize != null) {
				this.getTextState().setTextFontSize(fontSize.getReal());
			}
		}
	}

	@Override
	public GraphicsState clone() {
		GraphicsState clone = new GraphicsState();
		clone.CTM = this.CTM.clone();
		clone.textState = this.textState.clone();
		clone.fillColor = this.fillColor;
		clone.fillColorSpace = this.fillColorSpace;
		clone.processColorOperators = this.processColorOperators;
		clone.lineWidth = this.lineWidth;
		clone.lineCap = this.lineCap;
		return clone;
	}
}
